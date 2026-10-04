using System.Collections.Generic;
using System.IO;
using System.Linq;
using Stella.Level03;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Stella.EditorTools
{
    /// <summary>Builds the small, reference-safe Level 3 placeholder scene.</summary>
    [InitializeOnLoad]
    public static class Level03SceneBuilder
    {
        public const string SceneAssetPath = "Assets/_Stella/Scenes/Level03_TheCollector.unity";

        private static readonly Color PageBackground = new Color(0.035f, 0.03f, 0.07f, 1f);
        private static readonly Color PanelBackground = new Color(0.10f, 0.08f, 0.16f, 0.98f);
        private static readonly Color LightText = new Color(0.94f, 0.92f, 0.98f, 1f);
        private static readonly Color AccentPurple = new Color(0.52f, 0.28f, 0.78f, 1f);
        private static readonly Color AccentMagenta = new Color(0.78f, 0.24f, 0.62f, 1f);

        static Level03SceneBuilder() { EditorApplication.delayCall += BuildSceneIfMissing; }

        [MenuItem("Stella/Build Level 3 Placeholder Scene")]
        public static void BuildScene()
        {
            EnsureFolder("Assets/_Stella");
            EnsureFolder("Assets/_Stella/Scenes");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Level03_TheCollector";

            GameObject root = new GameObject("Level03_TheCollector");
            Level03Controller controller = root.AddComponent<Level03Controller>();
            CreateMainCamera(root.transform);
            CreateEventSystem(root.transform);
            Canvas canvas = CreateCanvas(root.transform);
            GameObject background = CreatePanel("Background", canvas.transform, PageBackground);
            Stretch(background.GetComponent<RectTransform>());
            background.transform.SetAsFirstSibling();

            CreateTopBar(canvas.transform);

            Text positionText;
            Text axisHintText;
            Text amountText;
            Text noticeText;
            Button rowAxisButton;
            Button columnAxisButton;
            Text rowAxisLabel;
            Text columnAxisLabel;
            Button decreaseButton;
            Button increaseButton;
            Button confirmButton;
            GameObject retryPanel;
            Button retryButton;
            CreateMainRegion(
                canvas.transform,
                out positionText,
                out axisHintText,
                out amountText,
                out noticeText,
                out rowAxisButton,
                out columnAxisButton,
                out rowAxisLabel,
                out columnAxisLabel,
                out decreaseButton,
                out increaseButton,
                out confirmButton,
                out retryPanel,
                out retryButton);

            controller.Configure(
                positionText, axisHintText, amountText, noticeText,
                rowAxisButton, columnAxisButton, rowAxisLabel, columnAxisLabel,
                decreaseButton, increaseButton, confirmButton,
                retryPanel, retryButton);

            EditorSceneManager.SaveScene(scene, SceneAssetPath);
            AddSceneToBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Created Stella Level 3 placeholder scene at " + SceneAssetPath);
        }

        public static void BuildSceneFromCommandLine() { BuildScene(); }

        private static void BuildSceneIfMissing()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(SceneAssetPath) == null) BuildScene();
        }

        private static Canvas CreateCanvas(Transform parent)
        {
            GameObject canvasObject = new GameObject(
                "Level03Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(parent, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600f, 900f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static void CreateTopBar(Transform parent)
        {
            GameObject top = CreatePanel("TopRegion", parent, PanelBackground);
            SetAnchors(top.GetComponent<RectTransform>(), new Vector2(0.035f, 0.885f), new Vector2(0.965f, 0.965f));
            HorizontalLayoutGroup layout = top.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 10, 10);
            layout.spacing = 14f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            // A fixed corner reserved for Stella's expressive portrait (thinking, reacting, etc.).
            GameObject stellaFace = CreatePanel("StellaFace", top.transform, AccentPurple);
            SetPreferredSize(stellaFace, new Vector2(64f, 64f));
            Text stellaFaceLabel = CreateText("StellaFaceLabel", stellaFace.transform, "STELLA", 11, TextAnchor.MiddleCenter);
            Stretch(stellaFaceLabel.rectTransform, 4f);

            Text rules = CreateText(
                "RulesText", top.transform,
                "THE COLLECTOR\nEach turn, lower the ROW or the COLUMN (never both). Land the token exactly on (0, 0) before the Collector does.",
                16, TextAnchor.MiddleLeft);
            rules.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
        }

        private static void CreateMainRegion(
            Transform parent,
            out Text positionText,
            out Text axisHintText,
            out Text amountText,
            out Text noticeText,
            out Button rowAxisButton,
            out Button columnAxisButton,
            out Text rowAxisLabel,
            out Text columnAxisLabel,
            out Button decreaseButton,
            out Button increaseButton,
            out Button confirmButton,
            out GameObject retryPanel,
            out Button retryButton)
        {
            GameObject main = CreatePanel("MainRegion", parent, PanelBackground);
            SetAnchors(main.GetComponent<RectTransform>(), new Vector2(0.035f, 0.035f), new Vector2(0.965f, 0.825f));
            HorizontalLayoutGroup mainLayout = main.AddComponent<HorizontalLayoutGroup>();
            mainLayout.padding = new RectOffset(16, 16, 14, 14);
            mainLayout.spacing = 18f;
            mainLayout.childControlWidth = true;
            mainLayout.childControlHeight = true;
            mainLayout.childForceExpandWidth = false;
            mainLayout.childForceExpandHeight = true;

            // Left: the holographic table. A plain placeholder stands in for the art pass.
            GameObject table = CreatePanel("HolographicTable", main.transform, new Color(0.07f, 0.06f, 0.12f, 1f));
            table.AddComponent<LayoutElement>().flexibleWidth = 1f;
            VerticalLayoutGroup tableLayout = table.AddComponent<VerticalLayoutGroup>();
            tableLayout.childAlignment = TextAnchor.MiddleCenter;
            tableLayout.spacing = 12f;
            tableLayout.childControlWidth = true;
            tableLayout.childControlHeight = true;
            tableLayout.childForceExpandWidth = false;
            tableLayout.childForceExpandHeight = false;

            CreateText("TableLabel", table.transform, "HOLOGRAPHIC TABLE", 18, TextAnchor.MiddleCenter)
                .gameObject.AddComponent<LayoutElement>().preferredHeight = 24f;

            GameObject tokenPanel = CreatePanel("TokenPanel", table.transform, AccentPurple);
            SetPreferredSize(tokenPanel, new Vector2(220f, 220f));
            Text tokenLabel = CreateText("TokenLabel", tokenPanel.transform, "TOKEN", 16, TextAnchor.MiddleCenter);
            Stretch(tokenLabel.rectTransform, 8f);

            // Right: pick a coordinate, pick an amount, confirm.
            GameObject controls = CreatePanel("ControlsRegion", main.transform, new Color(0.08f, 0.07f, 0.14f, 1f));
            LayoutElement controlsElement = controls.AddComponent<LayoutElement>();
            controlsElement.preferredWidth = 420f;
            controlsElement.minWidth = 360f;
            VerticalLayoutGroup controlsLayout = controls.AddComponent<VerticalLayoutGroup>();
            controlsLayout.padding = new RectOffset(16, 16, 14, 14);
            controlsLayout.spacing = 12f;
            controlsLayout.childAlignment = TextAnchor.UpperCenter;
            controlsLayout.childControlWidth = true;
            controlsLayout.childControlHeight = true;
            controlsLayout.childForceExpandWidth = true;
            controlsLayout.childForceExpandHeight = false;

            positionText = CreateText("PositionText", controls.transform, "(7, 10)", 24, TextAnchor.MiddleCenter);
            positionText.gameObject.AddComponent<LayoutElement>().preferredHeight = 30f;

            CreateText("ControlsHeading", controls.transform, "SLIDE THE TOKEN", 19, TextAnchor.MiddleCenter)
                .gameObject.AddComponent<LayoutElement>().preferredHeight = 24f;

            GameObject axisRow = new GameObject("AxisRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            axisRow.transform.SetParent(controls.transform, false);
            axisRow.GetComponent<LayoutElement>().preferredHeight = 64f;
            HorizontalLayoutGroup axisLayout = axisRow.GetComponent<HorizontalLayoutGroup>();
            axisLayout.spacing = 10f;
            axisLayout.childControlWidth = true;
            axisLayout.childControlHeight = true;
            axisLayout.childForceExpandWidth = true;
            axisLayout.childForceExpandHeight = false;
            rowAxisButton = CreateButton("RowAxisButton", axisRow.transform, "ROW: 7", AccentPurple, 18);
            rowAxisButton.gameObject.AddComponent<LayoutElement>().preferredHeight = 48f;
            rowAxisLabel = rowAxisButton.GetComponentInChildren<Text>();
            columnAxisButton = CreateButton("ColumnAxisButton", axisRow.transform, "COLUMN: 10", AccentMagenta, 18);
            columnAxisButton.gameObject.AddComponent<LayoutElement>().preferredHeight = 48f;
            columnAxisLabel = columnAxisButton.GetComponentInChildren<Text>();

            axisHintText = CreateText("AxisHintText", controls.transform, "REDUCING THE ROW", 16, TextAnchor.MiddleCenter);
            axisHintText.gameObject.AddComponent<LayoutElement>().preferredHeight = 22f;

            GameObject stepper = CreatePanel("AmountStepper", controls.transform, new Color(0.12f, 0.10f, 0.18f, 1f));
            LayoutElement stepperElement = stepper.AddComponent<LayoutElement>();
            stepperElement.preferredHeight = 100f;
            HorizontalLayoutGroup stepperLayout = stepper.AddComponent<HorizontalLayoutGroup>();
            stepperLayout.padding = new RectOffset(14, 14, 10, 10);
            stepperLayout.spacing = 14f;
            stepperLayout.childAlignment = TextAnchor.MiddleCenter;
            stepperLayout.childControlWidth = true;
            stepperLayout.childControlHeight = true;
            stepperLayout.childForceExpandWidth = false;
            stepperLayout.childForceExpandHeight = false;

            decreaseButton = CreateButton("DecreaseButton", stepper.transform, "-", new Color(0.26f, 0.24f, 0.34f, 1f), 28);
            SetPreferredSize(decreaseButton.gameObject, new Vector2(80f, 80f));

            GameObject amountPanel = CreatePanel("AmountPanel", stepper.transform, AccentPurple);
            SetPreferredSize(amountPanel, new Vector2(140f, 80f));
            amountText = CreateText("AmountText", amountPanel.transform, "1", 32, TextAnchor.MiddleCenter);
            Stretch(amountText.rectTransform, 4f);

            increaseButton = CreateButton("IncreaseButton", stepper.transform, "+", new Color(0.26f, 0.24f, 0.34f, 1f), 28);
            SetPreferredSize(increaseButton.gameObject, new Vector2(80f, 80f));

            confirmButton = CreateButton("ConfirmButton", controls.transform, "SLIDE TOKEN", AccentMagenta, 22);
            SetPreferredSize(confirmButton.gameObject, new Vector2(300f, 64f));

            noticeText = CreateText("NoticeText", controls.transform, string.Empty, 18, TextAnchor.MiddleCenter);
            noticeText.gameObject.AddComponent<LayoutElement>().preferredHeight = 50f;

            retryPanel = CreatePanel("RetryPanel", controls.transform, new Color(0.36f, 0.16f, 0.18f, 1f));
            SetPreferredSize(retryPanel, new Vector2(300f, 60f));
            retryButton = CreateButton("RetryButton", retryPanel.transform, "RETRY", new Color(0.73f, 0.25f, 0.25f, 1f), 18);
            Stretch(retryButton.GetComponent<RectTransform>(), 8f);
            retryPanel.SetActive(false);
        }

        private static Camera CreateMainCamera(Transform parent)
        {
            GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.transform.SetParent(parent, false);
            cameraObject.transform.localPosition = new Vector3(0f, 0f, -10f);
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = PageBackground;
            return camera;
        }

        private static void CreateEventSystem(Transform parent)
        {
            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            eventSystem.transform.SetParent(parent, false);
        }

        private static GameObject CreatePanel(string name, Transform parent, Color color)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            Image image = panel.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return panel;
        }

        private static Button CreateButton(string name, Transform parent, string label, Color color, int fontSize)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            Image image = buttonObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = true;
            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = color * 1.12f;
            colors.pressedColor = color * 0.82f;
            colors.disabledColor = new Color(0.34f, 0.33f, 0.38f, 0.9f);
            button.colors = colors;
            Text text = CreateText("Label", buttonObject.transform, label, fontSize, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform, 6f);
            text.raycastTarget = false;
            return button;
        }

        private static Text CreateText(string name, Transform parent, string content, int fontSize, TextAnchor alignment)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = LightText;
            text.supportRichText = false;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static void SetPreferredSize(GameObject gameObject, Vector2 size)
        {
            LayoutElement element = gameObject.GetComponent<LayoutElement>() ?? gameObject.AddComponent<LayoutElement>();
            element.preferredWidth = size.x;
            element.preferredHeight = size.y;
        }

        private static void Stretch(RectTransform rectTransform, float inset = 0f)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = new Vector2(inset, inset);
            rectTransform.offsetMax = new Vector2(-inset, -inset);
        }

        private static void SetAnchors(RectTransform rectTransform, Vector2 minimum, Vector2 maximum)
        {
            rectTransform.anchorMin = minimum;
            rectTransform.anchorMax = maximum;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private static void EnsureFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath)) return;
            string parent = Path.GetDirectoryName(assetPath).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(assetPath));
        }

        private static void AddSceneToBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.All(scene => scene.path != SceneAssetPath))
                scenes.Add(new EditorBuildSettingsScene(SceneAssetPath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
