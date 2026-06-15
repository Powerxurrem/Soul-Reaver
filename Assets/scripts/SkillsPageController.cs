using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SkillsPageController : MonoBehaviour
{
    [System.Serializable]
    public class SkillDefinition
    {
        public string name;
        public string description;
        public string damage;
        public string spellCritChance;
        public string cooldown;
        public string manaCost;

        public SkillDefinition(
            string name,
            string description,
            string damage,
            string spellCritChance,
            string cooldown,
            string manaCost
        )
        {
            this.name = name;
            this.description = description;
            this.damage = damage;
            this.spellCritChance = spellCritChance;
            this.cooldown = cooldown;
            this.manaCost = manaCost;
        }
    }

    static readonly Color PageColor = new Color(0.035f, 0.025f, 0.065f, 0.98f);
    static readonly Color CardColor = new Color(0.09f, 0.075f, 0.14f, 0.98f);
    static readonly Color CommonColor = new Color(0.72f, 0.75f, 0.79f);
    static readonly Color RareColor = new Color(0.25f, 0.58f, 0.95f);
    static readonly Color EpicColor = new Color(0.68f, 0.34f, 0.92f);
    static readonly Color LegendaryColor = new Color(1f, 0.62f, 0.16f);

    [Header("Skill Icons (Optional)")]
    public Sprite fireballIcon;
    public Sprite shadowBoltIcon;

    GameObject skillsPage;
    GameObject skillDetailPanel;
    GameObject ziggurathHub;
    Button openButton;
    Image detailIconImage;
    Image detailIconFrameImage;
    TextMeshProUGUI detailIconPlaceholder;
    TextMeshProUGUI detailTitle;
    TextMeshProUGUI detailDescription;
    TextMeshProUGUI detailDamage;
    TextMeshProUGUI detailCritChance;
    TextMeshProUGUI detailCooldown;
    TextMeshProUGUI detailManaCost;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Install()
    {
        Transform controllerObject = FindSceneTransform("SkillsPageController");
        SkillsPageController existingController =
            controllerObject != null ? controllerObject.GetComponent<SkillsPageController>() : null;

        if (existingController != null)
        {
            existingController.Initialize();
            return;
        }

        Transform skillsTile = FindSceneTransform("ZigguHubSkills");

        if (skillsTile == null)
            skillsTile = FindSceneTransform("ZigguHubSkillsMain");

        if (skillsTile == null)
            skillsTile = FindSkillsTile();

        if (skillsTile == null || skillsTile.GetComponent<SkillsPageController>() != null)
            return;

        skillsTile.gameObject.AddComponent<SkillsPageController>();
    }

    void Awake()
    {
        Initialize();
    }

    void Initialize()
    {
        ziggurathHub = FindSceneTransform("ZiggurathImage")?.gameObject;
        Transform skillsTile = FindSkillsTile();

        if (skillsTile == null)
        {
            Debug.LogWarning("[Skills Page] Could not find the Skills tile under ZiggurathImage.");
            return;
        }

        Button tileButton = skillsTile.GetComponent<Button>();

        if (tileButton == null)
            tileButton = skillsTile.gameObject.AddComponent<Button>();

        if (openButton != null && openButton != tileButton)
            openButton.onClick.RemoveListener(OpenSkillsPage);

        openButton = tileButton;

        Graphic clickGraphic = skillsTile.GetComponent<Graphic>();

        if (clickGraphic == null)
            clickGraphic = skillsTile.GetComponentInChildren<Graphic>(true);

        openButton.targetGraphic = clickGraphic;
        openButton.onClick.RemoveListener(OpenSkillsPage);
        openButton.onClick.AddListener(OpenSkillsPage);

        if (clickGraphic != null)
            clickGraphic.raycastTarget = true;

        Debug.Log($"[Skills Page] Click linked to {skillsTile.name}.");
    }

    void OpenSkillsPage()
    {
        if (skillsPage == null)
            BuildSkillsPage();

        skillsPage.SetActive(true);
        skillsPage.transform.SetAsLastSibling();

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    void CloseSkillsPage()
    {
        CloseSkillDetails();

        if (skillsPage != null)
            skillsPage.SetActive(false);

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    void BuildSkillsPage()
    {
        Transform pageParent = ziggurathHub != null ? ziggurathHub.transform : transform.root;

        Transform existingPage = FindDirectChild(pageParent, "SkillsRarityPage");

        if (existingPage != null)
        {
            skillsPage = existingPage.gameObject;

            for (int i = existingPage.childCount - 1; i >= 0; i--)
                Destroy(existingPage.GetChild(i).gameObject);
        }
        else
        {
            skillsPage = CreateUIObject("SkillsRarityPage", pageParent);
        }

        RectTransform pageRect = skillsPage.GetComponent<RectTransform>();
        pageRect.anchorMin = Vector2.zero;
        pageRect.anchorMax = Vector2.one;
        pageRect.offsetMin = new Vector2(14f, 7f);
        pageRect.offsetMax = new Vector2(-14f, -273f);
        pageRect.localScale = Vector3.one;

        Image pageImage = skillsPage.GetComponent<Image>();

        if (pageImage == null)
            pageImage = AddImage(skillsPage, PageColor);
        else
            pageImage.color = PageColor;

        CreateHeader(skillsPage.transform);
        CreateScrollArea(skillsPage.transform);
        CreateSkillDetailPanel(skillsPage.transform);
    }

    void CreateHeader(Transform parent)
    {
        GameObject header = CreateUIObject("Header", parent);
        RectTransform headerRect = header.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0f, 1f);
        headerRect.anchorMax = Vector2.one;
        headerRect.pivot = new Vector2(0.5f, 1f);
        headerRect.offsetMin = new Vector2(12f, -72f);
        headerRect.offsetMax = new Vector2(-12f, -10f);
        AddImage(header, new Color(0.12f, 0.075f, 0.18f, 0.98f));

        TextMeshProUGUI title = CreateText("Title", header.transform, "SKILLS", 34, Color.white);
        Stretch(title.rectTransform);
        title.alignment = TextAlignmentOptions.Center;
        title.fontStyle = FontStyles.Bold;
        title.characterSpacing = 3f;

        GameObject backObject = CreateUIObject("BackButton", header.transform);
        RectTransform backRect = backObject.GetComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0f, 0.5f);
        backRect.anchorMax = new Vector2(0f, 0.5f);
        backRect.pivot = new Vector2(0f, 0.5f);
        backRect.anchoredPosition = new Vector2(12f, 0f);
        backRect.sizeDelta = new Vector2(108f, 48f);
        Image backImage = AddImage(backObject, new Color(0.24f, 0.16f, 0.32f));

        Button backButton = backObject.AddComponent<Button>();
        backButton.targetGraphic = backImage;
        backButton.onClick.AddListener(CloseSkillsPage);

        TextMeshProUGUI backText = CreateText("Label", backObject.transform, "<  BACK", 19, Color.white);
        Stretch(backText.rectTransform);
        backText.alignment = TextAlignmentOptions.Center;
        backText.fontStyle = FontStyles.Bold;
    }

    void CreateScrollArea(Transform parent)
    {
        GameObject scrollObject = CreateUIObject("SkillsScrollView", parent);
        RectTransform scrollRectTransform = scrollObject.GetComponent<RectTransform>();
        Stretch(scrollRectTransform);
        scrollRectTransform.offsetMin = new Vector2(12f, 12f);
        scrollRectTransform.offsetMax = new Vector2(-12f, -82f);
        AddImage(scrollObject, new Color(0.02f, 0.015f, 0.04f, 0.72f));
        scrollObject.AddComponent<RectMask2D>();

        ScrollRect scrollRect = scrollObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Elastic;
        scrollRect.scrollSensitivity = 28f;

        GameObject content = CreateUIObject("Content", scrollObject.transform);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = Vector2.one;
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = Vector2.zero;

        VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(18, 18, 18, 24);
        layout.spacing = 18f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scrollRect.content = contentRect;

        CreateSection(content.transform, "COMMON", CommonColor, new[]
        {
            new SkillDefinition("Shadow Bolt", "Launch a shard of condensed shadow energy.", "18-24", "8%", "2.5s", "12"),
            new SkillDefinition("Grave Ward", "Gain a small shield after defeating an enemy.", "25 shield", "N/A", "8s", "16"),
            new SkillDefinition("Bone Edge", "Basic attacks deal increased physical damage.", "+12%", "5%", "Passive", "0"),
            new SkillDefinition("Fireball", "Hurl a burning orb that scorches its target.", "26-34", "10%", "4s", "18")
        });

        CreateSection(content.transform, "RARE", RareColor, new[]
        {
            new SkillDefinition("Wraith Step", "Briefly increase attack speed after a critical hit.", "+20% speed", "N/A", "6s", "10"),
            new SkillDefinition("Soul Siphon", "Restore health when soul damage is dealt.", "14-20", "12%", "5s", "15"),
            new SkillDefinition("Frozen Crypt", "Attacks have a chance to slow enemies.", "22-28", "8%", "7s", "20")
        });

        CreateSection(content.transform, "EPIC", EpicColor, new[]
        {
            new SkillDefinition("Death Nova", "Defeated enemies damage nearby targets.", "40-55", "15%", "3s", "0"),
            new SkillDefinition("Undying Will", "Survive fatal damage once per encounter.", "N/A", "N/A", "60s", "0"),
            new SkillDefinition("Void Harvest", "Critical kills grant a stacking damage bonus.", "+6% stack", "N/A", "Passive", "0")
        });

        CreateSection(content.transform, "LEGENDARY", LegendaryColor, new[]
        {
            new SkillDefinition("Reaper's Crown", "Every tenth attack summons a spectral scythe.", "90-120", "20%", "Passive", "0"),
            new SkillDefinition("Army of Night", "Periodically summon shades to attack with you.", "35 per shade", "10%", "18s", "35"),
            new SkillDefinition("Soul Reaver", "Consume stored souls to unleash devastating damage.", "180-240", "25%", "30s", "50")
        });
    }

    void CreateSection(Transform parent, string rarity, Color rarityColor, SkillDefinition[] skills)
    {
        GameObject section = CreateUIObject(rarity + "Section", parent);
        VerticalLayoutGroup sectionLayout = section.AddComponent<VerticalLayoutGroup>();
        sectionLayout.spacing = 10f;
        sectionLayout.childControlHeight = true;
        sectionLayout.childControlWidth = true;
        sectionLayout.childForceExpandHeight = false;
        sectionLayout.childForceExpandWidth = true;

        ContentSizeFitter fitter = section.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        TextMeshProUGUI heading = CreateText("Heading", section.transform, rarity, 25, rarityColor);
        heading.fontStyle = FontStyles.Bold;
        heading.characterSpacing = 2f;
        heading.alignment = TextAlignmentOptions.MidlineLeft;
        LayoutElement headingLayout = heading.gameObject.AddComponent<LayoutElement>();
        headingLayout.preferredHeight = 40f;

        GameObject row = CreateUIObject("SkillCards", section.transform);
        GridLayoutGroup rowLayout = row.AddComponent<GridLayoutGroup>();
        rowLayout.cellSize = new Vector2(214f, 214f);
        rowLayout.spacing = new Vector2(16f, 16f);
        rowLayout.childAlignment = TextAnchor.UpperCenter;
        rowLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        rowLayout.constraintCount = 2;
        LayoutElement rowElement = row.AddComponent<LayoutElement>();
        int rowCount = Mathf.CeilToInt(skills.Length / 2f);
        rowElement.preferredHeight = (rowCount * 214f) + (Mathf.Max(0, rowCount - 1) * 16f);

        foreach (SkillDefinition skill in skills)
            CreateSkillCard(row.transform, skill, rarityColor);
    }

    void CreateSkillCard(Transform parent, SkillDefinition skill, Color rarityColor)
    {
        GameObject card = CreateUIObject(skill.name, parent);
        Image cardImage = AddImage(card, CardColor);
        Outline outline = card.AddComponent<Outline>();
        outline.effectColor = rarityColor;
        outline.effectDistance = new Vector2(2f, -2f);

        Button cardButton = card.AddComponent<Button>();
        cardButton.targetGraphic = cardImage;
        cardButton.onClick.AddListener(() => OpenSkillDetails(skill, rarityColor));

        VerticalLayoutGroup layout = card.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(14, 14, 14, 14);
        layout.spacing = 10f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        GameObject iconFrame = CreateUIObject("IconFrame", card.transform);
        Image iconFrameImage = AddImage(
            iconFrame,
            new Color(rarityColor.r, rarityColor.g, rarityColor.b, 0.16f)
        );
        iconFrameImage.raycastTarget = false;
        LayoutElement iconLayout = iconFrame.AddComponent<LayoutElement>();
        iconLayout.preferredWidth = 132f;
        iconLayout.preferredHeight = 132f;

        Sprite skillIcon = GetSkillIcon(skill.name);

        if (skillIcon != null)
        {
            GameObject iconObject = CreateUIObject("Icon", iconFrame.transform);
            RectTransform iconRect = iconObject.GetComponent<RectTransform>();
            Stretch(iconRect);
            iconRect.offsetMin = new Vector2(6f, 6f);
            iconRect.offsetMax = new Vector2(-6f, -6f);
            Image iconImage = AddImage(iconObject, Color.white);
            iconImage.sprite = skillIcon;
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;
        }
        else
        {
            TextMeshProUGUI placeholder = CreateText(
                "Placeholder",
                iconFrame.transform,
                GetInitials(skill.name),
                28,
                rarityColor
            );
            Stretch(placeholder.rectTransform);
            placeholder.alignment = TextAlignmentOptions.Center;
            placeholder.fontStyle = FontStyles.Bold;
        }

        TextMeshProUGUI nameText = CreateText("Name", card.transform, skill.name, 20, Color.white);
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.fontStyle = FontStyles.Bold;
        LayoutElement nameLayout = nameText.gameObject.AddComponent<LayoutElement>();
        nameLayout.preferredHeight = 32f;
    }

    void CreateSkillDetailPanel(Transform parent)
    {
        skillDetailPanel = CreateUIObject("SkillDetailPanel", parent);
        RectTransform panelRect = skillDetailPanel.GetComponent<RectTransform>();
        Stretch(panelRect);
        AddImage(skillDetailPanel, PageColor);

        GameObject header = CreateUIObject("DetailHeader", skillDetailPanel.transform);
        RectTransform headerRect = header.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0f, 1f);
        headerRect.anchorMax = Vector2.one;
        headerRect.pivot = new Vector2(0.5f, 1f);
        headerRect.offsetMin = new Vector2(12f, -72f);
        headerRect.offsetMax = new Vector2(-12f, -10f);
        AddImage(header, new Color(0.12f, 0.075f, 0.18f, 0.98f));

        GameObject backObject = CreateUIObject("BackToSkillsButton", header.transform);
        RectTransform backRect = backObject.GetComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0f, 0.5f);
        backRect.anchorMax = new Vector2(0f, 0.5f);
        backRect.pivot = new Vector2(0f, 0.5f);
        backRect.anchoredPosition = new Vector2(12f, 0f);
        backRect.sizeDelta = new Vector2(154f, 48f);
        Image backImage = AddImage(backObject, new Color(0.24f, 0.16f, 0.32f));

        Button backButton = backObject.AddComponent<Button>();
        backButton.targetGraphic = backImage;
        backButton.onClick.AddListener(CloseSkillDetails);

        TextMeshProUGUI backText = CreateText(
            "Label",
            backObject.transform,
            "<  SKILLS",
            18,
            Color.white
        );
        Stretch(backText.rectTransform);
        backText.alignment = TextAlignmentOptions.Center;
        backText.fontStyle = FontStyles.Bold;

        detailTitle = CreateText("Title", header.transform, "SKILL", 30, Color.white);
        Stretch(detailTitle.rectTransform);
        detailTitle.alignment = TextAlignmentOptions.Center;
        detailTitle.fontStyle = FontStyles.Bold;

        GameObject content = CreateUIObject("DetailContent", skillDetailPanel.transform);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        Stretch(contentRect);
        contentRect.offsetMin = new Vector2(28f, 24f);
        contentRect.offsetMax = new Vector2(-28f, -92f);

        VerticalLayoutGroup contentLayout = content.AddComponent<VerticalLayoutGroup>();
        contentLayout.spacing = 14f;
        contentLayout.childAlignment = TextAnchor.UpperCenter;
        contentLayout.childControlHeight = true;
        contentLayout.childControlWidth = true;
        contentLayout.childForceExpandHeight = false;
        contentLayout.childForceExpandWidth = false;

        GameObject iconFrame = CreateUIObject("LargeIconFrame", content.transform);
        detailIconFrameImage = AddImage(iconFrame, new Color(1f, 1f, 1f, 0.12f));
        LayoutElement iconFrameLayout = iconFrame.AddComponent<LayoutElement>();
        iconFrameLayout.preferredWidth = 150f;
        iconFrameLayout.preferredHeight = 150f;

        GameObject iconObject = CreateUIObject("Icon", iconFrame.transform);
        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        Stretch(iconRect);
        iconRect.offsetMin = new Vector2(10f, 10f);
        iconRect.offsetMax = new Vector2(-10f, -10f);
        detailIconImage = AddImage(iconObject, Color.white);
        detailIconImage.preserveAspect = true;
        detailIconImage.raycastTarget = false;

        detailIconPlaceholder = CreateText("Placeholder", iconFrame.transform, "SB", 44, Color.white);
        Stretch(detailIconPlaceholder.rectTransform);
        detailIconPlaceholder.alignment = TextAlignmentOptions.Center;
        detailIconPlaceholder.fontStyle = FontStyles.Bold;

        detailDescription = CreateText(
            "Description",
            content.transform,
            string.Empty,
            18,
            new Color(0.85f, 0.83f, 0.9f)
        );
        detailDescription.alignment = TextAlignmentOptions.Center;
        detailDescription.enableWordWrapping = true;
        LayoutElement descriptionLayout = detailDescription.gameObject.AddComponent<LayoutElement>();
        descriptionLayout.preferredWidth = 440f;
        descriptionLayout.preferredHeight = 66f;

        GameObject statsGrid = CreateUIObject("StatsGrid", content.transform);
        GridLayoutGroup grid = statsGrid.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(210f, 76f);
        grid.spacing = new Vector2(14f, 14f);
        grid.childAlignment = TextAnchor.UpperCenter;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 2;
        LayoutElement statsLayout = statsGrid.AddComponent<LayoutElement>();
        statsLayout.preferredWidth = 434f;
        statsLayout.preferredHeight = 166f;

        detailDamage = CreateDetailStat(statsGrid.transform, "DAMAGE");
        detailCritChance = CreateDetailStat(statsGrid.transform, "SPELL CRIT CHANCE");
        detailCooldown = CreateDetailStat(statsGrid.transform, "COOLDOWN");
        detailManaCost = CreateDetailStat(statsGrid.transform, "MANA COST");

        skillDetailPanel.SetActive(false);
    }

    TextMeshProUGUI CreateDetailStat(Transform parent, string label)
    {
        GameObject statCard = CreateUIObject(label, parent);
        AddImage(statCard, new Color(0.09f, 0.075f, 0.14f, 0.98f));

        VerticalLayoutGroup layout = statCard.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(8, 8, 8, 8);
        layout.spacing = 2f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;

        TextMeshProUGUI labelText = CreateText("Label", statCard.transform, label, 13, CommonColor);
        labelText.alignment = TextAlignmentOptions.Center;
        labelText.fontStyle = FontStyles.Bold;
        LayoutElement labelLayout = labelText.gameObject.AddComponent<LayoutElement>();
        labelLayout.preferredHeight = 22f;

        TextMeshProUGUI valueText = CreateText("Value", statCard.transform, "-", 22, Color.white);
        valueText.alignment = TextAlignmentOptions.Center;
        valueText.fontStyle = FontStyles.Bold;
        LayoutElement valueLayout = valueText.gameObject.AddComponent<LayoutElement>();
        valueLayout.preferredHeight = 32f;
        return valueText;
    }

    void OpenSkillDetails(SkillDefinition skill, Color rarityColor)
    {
        if (skillDetailPanel == null)
            return;

        detailTitle.text = skill.name.ToUpperInvariant();
        detailTitle.color = rarityColor;
        detailDescription.text = skill.description;
        detailDamage.text = skill.damage;
        detailCritChance.text = skill.spellCritChance;
        detailCooldown.text = skill.cooldown;
        detailManaCost.text = skill.manaCost;
        detailIconFrameImage.color = new Color(rarityColor.r, rarityColor.g, rarityColor.b, 0.2f);

        Sprite icon = GetSkillIcon(skill.name);
        detailIconImage.sprite = icon;
        detailIconImage.gameObject.SetActive(icon != null);
        detailIconPlaceholder.gameObject.SetActive(icon == null);
        detailIconPlaceholder.text = GetInitials(skill.name);
        detailIconPlaceholder.color = rarityColor;

        skillDetailPanel.SetActive(true);
        skillDetailPanel.transform.SetAsLastSibling();

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    void CloseSkillDetails()
    {
        if (skillDetailPanel != null)
            skillDetailPanel.SetActive(false);

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    Sprite GetSkillIcon(string skillName)
    {
        if (skillName == "Fireball")
            return fireballIcon;

        if (skillName == "Shadow Bolt")
            return shadowBoltIcon;

        return null;
    }

    static string GetInitials(string value)
    {
        string[] words = value.Split(' ');

        if (words.Length == 1)
            return words[0].Substring(0, Mathf.Min(2, words[0].Length)).ToUpperInvariant();

        return (words[0][0].ToString() + words[words.Length - 1][0]).ToUpperInvariant();
    }

    static GameObject CreateUIObject(string objectName, Transform parent)
    {
        GameObject uiObject = new GameObject(objectName, typeof(RectTransform));
        uiObject.layer = 5;
        uiObject.transform.SetParent(parent, false);
        return uiObject;
    }

    static Image AddImage(GameObject target, Color color)
    {
        Image image = target.AddComponent<Image>();
        image.color = color;
        return image;
    }

    static TextMeshProUGUI CreateText(
        string objectName,
        Transform parent,
        string value,
        float fontSize,
        Color color
    )
    {
        GameObject textObject = CreateUIObject(objectName, parent);
        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.color = color;
        text.raycastTarget = false;
        text.overflowMode = TextOverflowModes.Ellipsis;
        return text;
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    static Transform FindSceneTransform(string objectName)
    {
        Scene scene = SceneManager.GetActiveScene();

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform result = FindChild(root.transform, objectName);

            if (result != null)
                return result;
        }

        return null;
    }

    static Transform FindChild(Transform parent, string objectName)
    {
        if (parent.name == objectName)
            return parent;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform result = FindChild(parent.GetChild(i), objectName);

            if (result != null)
                return result;
        }

        return null;
    }

    static Transform FindDirectChild(Transform parent, string objectName)
    {
        if (parent == null)
            return null;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);

            if (child.name == objectName)
                return child;
        }

        return null;
    }

    static Transform FindSkillsTile()
    {
        Transform ziggurath = FindSceneTransform("ZiggurathImage");

        if (ziggurath == null)
            return null;

        Transform namedTile = FindDirectChild(ziggurath, "ZigguHubSkillsMain");

        if (namedTile == null)
            namedTile = FindDirectChild(ziggurath, "ZigguHubSkills");

        if (namedTile == null)
            namedTile = FindDirectChild(ziggurath, "SkillsPage");

        return namedTile;
    }
}
