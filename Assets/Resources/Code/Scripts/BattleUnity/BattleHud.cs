using System.Collections;
using System.Collections.Generic;
using Shadowkeep.Battle;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Интерфейс боя, собираемый из кода: панель действий, полоски здоровья над бойцами, выбор цели,
/// кнопки «Авто» и «x2», журнал и итог. Работает без настройки сцены; позже заменяется нарисованным интерфейсом
/// (достаточно сохранить те же публичные методы).
/// </summary>
public sealed class BattleHud : MonoBehaviour
{
    private sealed class Widget
    {
        public Combatant C; public UnitView View;
        public RectTransform Root, Fill; public Text Label; public Image Marker;
    }

    private sealed class ActionButton { public IBattleAction Action; public Button Button; public Text Label; }

    private BattleController controller;
    private Font font;
    private RectTransform canvasRect, actionBar;
    private Text apText, hintText, turnText, logText, autoLabel, speedLabel, resultLabel;
    private Button endTurnButton, cancelButton;
    private GameObject resultPanel;
    private readonly Dictionary<Combatant, Widget> widgets = new Dictionary<Combatant, Widget>();
    private readonly List<ActionButton> actionButtons = new List<ActionButton>();
    private readonly List<string> logLines = new List<string>();
    private Combatant activeActor;
    private bool playerTurn;

    public static BattleHud Create(BattleController controller)
    {
        var go = new GameObject("BattleHud");
        var hud = go.AddComponent<BattleHud>();
        hud.Build(controller);
        return hud;
    }

    // ---------- Построение ----------

    private void Build(BattleController owner)
    {
        controller = owner;
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();
        canvasRect = (RectTransform)transform;

        // Нижняя панель действий
        actionBar = NewRect("ActionBar", transform);
        actionBar.anchorMin = new Vector2(0, 0); actionBar.anchorMax = new Vector2(1, 0);
        actionBar.pivot = new Vector2(0.5f, 0); actionBar.sizeDelta = new Vector2(0, 200); actionBar.anchoredPosition = Vector2.zero;
        actionBar.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, 0.6f);
        var row = actionBar.gameObject.AddComponent<HorizontalLayoutGroup>();
        row.spacing = 24; row.padding = new RectOffset(40, 40, 30, 30);
        row.childAlignment = TextAnchor.MiddleCenter;
        row.childControlWidth = false; row.childControlHeight = false;
        row.childForceExpandWidth = false; row.childForceExpandHeight = false;
        actionBar.gameObject.SetActive(false);

        apText = NewText(transform, "", 40, TextAnchor.MiddleCenter, Color.white);
        Place(apText.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 225), new Vector2(700, 60));
        hintText = NewText(transform, "", 38, TextAnchor.MiddleCenter, new Color(1f, 0.9f, 0.3f));
        Place(hintText.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 290), new Vector2(900, 60));

        turnText = NewText(transform, "", 38, TextAnchor.MiddleCenter, Color.white);
        Place(turnText.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -45), new Vector2(1000, 60));

        logText = NewText(transform, "", 30, TextAnchor.UpperLeft, Color.white);
        Place(logText.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(30, -30), new Vector2(900, 260));
        logText.rectTransform.pivot = new Vector2(0, 1);

        var autoBtn = NewButton(transform, "Авто", new Vector2(230, 90), controller.ToggleAuto, out autoLabel);
        Place((RectTransform)autoBtn.transform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-30, -30), new Vector2(230, 90));
        ((RectTransform)autoBtn.transform).pivot = new Vector2(1, 1);
        var speedBtn = NewButton(transform, "x1", new Vector2(150, 90), controller.ToggleSpeed, out speedLabel);
        Place((RectTransform)speedBtn.transform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-280, -30), new Vector2(150, 90));
        ((RectTransform)speedBtn.transform).pivot = new Vector2(1, 1);

        // Итоговая панель
        resultPanel = NewRect("Result", transform).gameObject;
        var rr = (RectTransform)resultPanel.transform;
        Stretch(rr);
        resultPanel.AddComponent<Image>().color = new Color(0, 0, 0, 0.7f);
        resultLabel = NewText(rr, "", 90, TextAnchor.MiddleCenter, Color.white);
        Place(resultLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 80), new Vector2(1400, 160));
        Text again;
        var againBtn = NewButton(rr, "Заново", new Vector2(400, 120), controller.RequestRestart, out again);
        Place((RectTransform)againBtn.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -100), new Vector2(400, 120));
        resultPanel.SetActive(false);
    }

    // ---------- Бойцы ----------

    public void Register(UnitView view)
    {
        var c = view.Combatant;
        var w = new Widget { C = c, View = view };

        w.Root = NewRect(c.Name + "_Hud", transform);
        w.Root.anchorMin = w.Root.anchorMax = new Vector2(0.5f, 0.5f);
        w.Root.sizeDelta = new Vector2(240, 70);

        var bar = NewRect("Bar", w.Root);
        bar.sizeDelta = new Vector2(220, 22); bar.anchoredPosition = new Vector2(0, -10);
        var barImg = bar.gameObject.AddComponent<Image>(); barImg.color = new Color(0.1f, 0.1f, 0.1f, 0.85f); barImg.raycastTarget = false;
        w.Fill = NewRect("Fill", bar);
        Stretch(w.Fill);
        var fillImg = w.Fill.gameObject.AddComponent<Image>(); fillImg.color = c.Team == Team.Heroes ? new Color(0.3f, 0.85f, 0.35f) : new Color(0.9f, 0.25f, 0.2f); fillImg.raycastTarget = false;

        w.Label = NewText(w.Root, "", 26, TextAnchor.MiddleCenter, Color.white);
        w.Label.rectTransform.anchoredPosition = new Vector2(0, 22);
        w.Label.rectTransform.sizeDelta = new Vector2(340, 36);

        var markerRect = NewRect("Marker", w.Root);
        markerRect.sizeDelta = new Vector2(34, 34); markerRect.anchoredPosition = new Vector2(0, 75);
        markerRect.localRotation = Quaternion.Euler(0, 0, 45);
        w.Marker = markerRect.gameObject.AddComponent<Image>();
        w.Marker.color = new Color(1f, 0.9f, 0.2f); w.Marker.raycastTarget = false;
        markerRect.gameObject.SetActive(false);

        widgets[c] = w;
        SetHp(c, c.Hp);
    }

    public void SetHp(Combatant c, int hp)
    {
        Widget w;
        if (!widgets.TryGetValue(c, out w)) return;
        float ratio = c.MaxHp > 0 ? Mathf.Clamp01((float)hp / c.MaxHp) : 0f;
        w.Fill.anchorMax = new Vector2(ratio, 1);
        w.Label.text = c.Name + "  " + hp + "/" + c.MaxHp;
    }

    public void SetActiveUnit(Combatant c)
    {
        foreach (var w in widgets.Values) w.Label.color = w.C == c ? new Color(1f, 0.9f, 0.3f) : Color.white;
    }

    public void SetTurnLabel(Combatant actor, int round) { turnText.text = "Раунд " + round + "  -  ходит: " + actor.Name; }

    public void OnDied(Combatant c)
    {
        Widget w;
        if (widgets.TryGetValue(c, out w)) w.Root.gameObject.SetActive(false);
    }

    // ---------- Ход игрока ----------

    public void ShowPlayerTurn(Combatant actor)
    {
        activeActor = actor;
        playerTurn = true;

        foreach (var ab in actionButtons) Destroy(ab.Button.gameObject);
        actionButtons.Clear();
        if (endTurnButton != null) Destroy(endTurnButton.gameObject);
        if (cancelButton != null) Destroy(cancelButton.gameObject);

        foreach (var action in actor.Actions)
        {
            var a = action;
            Text label;
            var btn = NewButton(actionBar, a.Name, new Vector2(300, 110), () => controller.TurnInput.SelectAction(a), out label);
            actionButtons.Add(new ActionButton { Action = a, Button = btn, Label = label });
        }
        Text l1, l2;
        endTurnButton = NewButton(actionBar, "Конец хода", new Vector2(300, 110), controller.TurnInput.RequestEndTurn, out l1);
        cancelButton = NewButton(actionBar, "Отмена", new Vector2(220, 110), controller.TurnInput.Cancel, out l2);
        actionBar.gameObject.SetActive(true);
    }

    public void HidePlayerTurn()
    {
        playerTurn = false;
        actionBar.gameObject.SetActive(false);
        apText.text = "";
        hintText.text = "";
        HideTargets();
    }

    private void HideTargets()
    {
        foreach (var w in widgets.Values)
        {
            w.Marker.gameObject.SetActive(false);
            if (w.View != null) w.View.SetHighlight(HighlightMode.None);
        }
    }

    // ---------- Журнал, всплывающий текст, итог ----------

    public void Log(string line)
    {
        logLines.Add(line);
        if (logLines.Count > 6) logLines.RemoveAt(0);
        logText.text = string.Join("\n", logLines);
    }

    public void Float(Combatant c, string text, Color color) { StartCoroutine(FloatRoutine(c, text, color)); }

    private IEnumerator FloatRoutine(Combatant c, string text, Color color)
    {
        Widget w;
        if (!widgets.TryGetValue(c, out w)) yield break;
        Vector2 start = w.Root.anchoredPosition + new Vector2(0, -60);
        var t = NewText(transform, text, 52, TextAnchor.MiddleCenter, color);
        var rt = t.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(300, 70);
        float duration = 0.9f / controller.Speed, elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float k = Mathf.Clamp01(elapsed / duration);
            rt.anchoredPosition = start + new Vector2(0, 90f * k);
            t.color = new Color(color.r, color.g, color.b, 1f - k);
            yield return null;
        }
        Destroy(t.gameObject);
    }

    public void ShowResult(BattleOutcome outcome)
    {
        HidePlayerTurn();
        resultLabel.text = outcome == BattleOutcome.HeroesWon ? "ПОБЕДА"
                         : outcome == BattleOutcome.EnemiesWon ? "ПОРАЖЕНИЕ" : "НИЧЬЯ (бой завис)";
        resultPanel.SetActive(true);
    }

    // ---------- Обновление каждый кадр ----------

    private void Update()
    {
        autoLabel.text = controller.Auto ? "Авто: ВКЛ" : "Авто: выкл";
        speedLabel.text = "x" + controller.Speed;

        if (!playerTurn || controller.Engine == null) return;
        var engine = controller.Engine;
        var input = controller.TurnInput;
        bool choosingTarget = input.PendingAction != null;

        apText.text = "Очки действий: " + activeActor.ActionPoints + "/" + activeActor.ActionPointsPerTurn;
        hintText.text = choosingTarget ? "Выбери цель" : "";

        foreach (var ab in actionButtons)
        {
            ab.Button.interactable = !choosingTarget && engine.CanUse(ab.Action);
            string extra = "";
            int cd = activeActor.CooldownOf(ab.Action);
            if (cd > 0) extra = " (КД " + cd + ")";
            var potion = ab.Action as PotionAction;
            if (potion != null) extra += " x" + potion.Charges;
            ab.Label.text = ab.Action.Name + "\n" + ab.Action.ApCost + " ОД" + extra;
        }
        endTurnButton.interactable = !choosingTarget;
        cancelButton.gameObject.SetActive(choosingTarget);

        if (choosingTarget) UpdateTargeting(input);
        else HideTargets(); // подсветку не сбрасываем каждый кадр, только когда выбор цели закончился
    }

    /// <summary>Подсвечивает доступные цели и принимает клик по самому спрайту (наведение мыши усиливает подсветку).</summary>
    private void UpdateTargeting(PlayerTurnInput input)
    {
        var cam = Camera.main;
        if (cam == null) return;

        Vector2 pointer = Input.mousePosition;
        bool overUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        Combatant hovered = null;
        float bestDistance = float.MaxValue;
        var targets = new List<Combatant>(input.ValidTargets(input.PendingAction));
        foreach (var other in widgets.Values)
        {
            if (targets.Contains(other.C)) continue;
            other.Marker.gameObject.SetActive(false);
            if (other.View != null) other.View.SetHighlight(HighlightMode.None);
        }
        foreach (var target in targets)
        {
            Widget w;
            if (!widgets.TryGetValue(target, out w) || w.View == null) continue;
            Rect rect = w.View.ScreenRect(cam);
            if (overUi || !rect.Contains(pointer)) continue;
            float d = (rect.center - pointer).sqrMagnitude; // при наложении выбираем ближайшего к клику
            if (d < bestDistance) { bestDistance = d; hovered = target; }
        }

        foreach (var target in targets)
        {
            Widget w;
            if (!widgets.TryGetValue(target, out w) || w.View == null) continue;
            w.Marker.gameObject.SetActive(true);
            w.View.SetHighlight(target == hovered ? HighlightMode.Hovered : HighlightMode.Selectable);
        }

        if (hovered != null && Input.GetMouseButtonDown(0) && input.PickTarget(hovered))
            widgets[hovered].View.PlaySelected();
    }

    private void LateUpdate()
    {
        var cam = Camera.main;
        if (cam == null) return;
        foreach (var w in widgets.Values)
        {
            if (w.View == null || !w.Root.gameObject.activeSelf) continue;
            Vector3 screen = cam.WorldToScreenPoint(w.View.HeadWorldPosition());
            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, new Vector2(screen.x, screen.y), null, out local);
            w.Root.anchoredPosition = local + new Vector2(0, 30);
        }
    }

    // ---------- Помощники построения ----------

    private static RectTransform NewRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    private static void Place(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
    {
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.anchoredPosition = position; rt.sizeDelta = size;
    }

    private Text NewText(Transform parent, string content, int size, TextAnchor anchor, Color color)
    {
        var rt = NewRect("Text", parent);
        var t = rt.gameObject.AddComponent<Text>();
        t.font = font; t.fontSize = size; t.alignment = anchor; t.color = color; t.text = content;
        t.horizontalOverflow = HorizontalWrapMode.Overflow; t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    private Button NewButton(Transform parent, string label, Vector2 size, UnityAction onClick, out Text labelText)
    {
        var rt = NewRect(label, parent);
        rt.sizeDelta = size;
        var img = rt.gameObject.AddComponent<Image>();
        img.color = new Color(0.3f, 0.22f, 0.12f, 0.95f);
        var btn = rt.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);
        var colors = btn.colors;
        colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.6f);
        btn.colors = colors;
        labelText = NewText(rt, label, 30, TextAnchor.MiddleCenter, Color.white);
        Stretch(labelText.rectTransform);
        return btn;
    }
}
