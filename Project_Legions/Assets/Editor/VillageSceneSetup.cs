using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;

namespace PPCorps.EditorTools
{
    public class VillageSceneSetup : EditorWindow
    {
        [MenuItem("砰砰军团/一键搭建村庄场景 (MVP)", false, 0)]
        public static void SetupVillageScene()
        {
            // 前置处理：确保所有 PNG 图片以 Sprite 类型导入
            EnsureSpritesAreSprite();

            AssetDatabase.Refresh();

            string scenePath = "Assets/Scenes/VillageScene.unity";

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SetActiveScene(scene);

            // ==================== 1. 主摄像机 ====================
            var camObj = CreateGameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            var cam = camObj.GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 11.6667f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.55f, 0.75f, 1f); // 天空蓝
            camObj.transform.position = new Vector3(0, -1.6667f, -10);
            camObj.tag = "MainCamera";
            var camCtrl = camObj.AddComponent<VillageCameraController>();
            camCtrl.SetBounds(-49.5f, 26.6f);
            SetPrivateField(camCtrl, "_scrollAmount", 40f);
            camObj.AddComponent<Physics2DRaycaster>();

            // ==================== 2. Canvas ====================
            var canvasObj = CreateGameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.planeDistance = 1;
            var scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // ==================== 3. EventSystem ====================
            CreateGameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            // ==================== 4. 资源栏 (顶部) ====================
            var resourceBarObj = CreateUIObject("ResourceBar", canvasObj.transform,
                new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(600, 50), new Vector2(0, -35));
            var resourceBar = resourceBarObj.AddComponent<ResourceBarUI>();

            // 金币
            var goldText = CreateResourceText(resourceBarObj.transform, "GoldLabel", "GoldText",
                new Vector2(-150, 0), "金币: 500", TextAnchor.MiddleLeft);

            // 宝石
            var gemText = CreateResourceText(resourceBarObj.transform, "GemLabel", "GemText",
                new Vector2(50, 0), "宝石: 100", TextAnchor.MiddleLeft);

            // 科技点
            var techText = CreateResourceText(resourceBarObj.transform, "TechLabel", "TechText",
                new Vector2(250, 0), "科技: 0", TextAnchor.MiddleLeft);

            // 使用反射设置私有字段
            SetPrivateField(resourceBar, "_goldText", goldText.GetComponent<Text>());
            SetPrivateField(resourceBar, "_gemsText", gemText.GetComponent<Text>());
            SetPrivateField(resourceBar, "_techText", techText.GetComponent<Text>());

            // ==================== 5. 底部工具栏 ====================
            var toolbarObj = CreateUIObject("Toolbar", canvasObj.transform,
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(500, 70), new Vector2(0, 40));
            var toolbarBg = toolbarObj.AddComponent<Image>();
            toolbarBg.color = new Color(0, 0, 0, 0.6f);
            var toolbar = toolbarObj.AddComponent<VillageToolbarUI>();

            var btnCombat = CreateToolbarButton(toolbarObj.transform, "Btn_Combat", "⚔ 战斗", -200);
            var btnDeck = CreateToolbarButton(toolbarObj.transform, "Btn_Deck", "🃏 卡牌", -70);
            var btnShop = CreateToolbarButton(toolbarObj.transform, "Btn_Shop", "🏪 商店", 60);
            var btnMail = CreateToolbarButton(toolbarObj.transform, "Btn_Mail", "✉ 邮件", 190);

            SetPrivateField(toolbar, "_combatBtn", btnCombat.GetComponent<Button>());
            SetPrivateField(toolbar, "_deckBtn", btnDeck.GetComponent<Button>());
            SetPrivateField(toolbar, "_shopBtn", btnShop.GetComponent<Button>());
            SetPrivateField(toolbar, "_mailBtn", btnMail.GetComponent<Button>());

            // ==================== 6. 左右滚动按钮 ====================
            var scrollLeftObj = CreateUIObject("Btn_ScrollLeft", canvasObj.transform,
                new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(60, 160), new Vector2(10, 0));
            var scrollLeftBtn = scrollLeftObj.AddComponent<Button>();
            var slColors = scrollLeftBtn.colors;
            slColors.normalColor = new Color(1, 1, 1, 0.25f);
            slColors.highlightedColor = new Color(1, 1, 1, 0.5f);
            scrollLeftBtn.colors = slColors;
            var slTxt = scrollLeftObj.AddComponent<Text>();
            slTxt.text = "◀";
            slTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            slTxt.fontSize = 48;
            slTxt.alignment = TextAnchor.MiddleCenter;
            slTxt.color = Color.white;
            scrollLeftBtn.targetGraphic = slTxt;
            var slScroll = scrollLeftObj.AddComponent<ScrollButton>();
            slScroll.direction = ScrollButton.Direction.Left;

            var scrollRightObj = CreateUIObject("Btn_ScrollRight", canvasObj.transform,
                new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f),
                new Vector2(60, 160), new Vector2(-10, 0));
            var scrollRightBtn = scrollRightObj.AddComponent<Button>();
            var srColors = scrollRightBtn.colors;
            srColors.normalColor = new Color(1, 1, 1, 0.25f);
            srColors.highlightedColor = new Color(1, 1, 1, 0.5f);
            scrollRightBtn.colors = srColors;
            var srTxt = scrollRightObj.AddComponent<Text>();
            srTxt.text = "▶";
            srTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            srTxt.fontSize = 48;
            srTxt.alignment = TextAnchor.MiddleCenter;
            srTxt.color = Color.white;
            scrollRightBtn.targetGraphic = srTxt;
            var srScroll = scrollRightObj.AddComponent<ScrollButton>();
            srScroll.direction = ScrollButton.Direction.Right;

            // ==================== 7. Panel 根容器 ====================
            var panelRoot = CreateUIObject("PanelRoot", canvasObj.transform,
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero);
            panelRoot.SetActive(false);

            // ==================== 8. Managers ====================
            var mgrObj = CreateGameObject("VillageManagers",
                typeof(VillageManager), typeof(VillageDataManager), typeof(BuildingManager));

            var vm = mgrObj.GetComponent<VillageManager>();
            var dm = mgrObj.GetComponent<VillageDataManager>();
            var bm = mgrObj.GetComponent<BuildingManager>();

            SetPrivateField(vm, "_panelRoot", panelRoot);

            // ==================== 9. 地面 ====================
            var groundObj = new GameObject("Ground");
            groundObj.transform.SetParent(mgrObj.transform);
            groundObj.transform.position = new Vector3(0, -11.6667f, 0.5f);
            var groundSR = groundObj.AddComponent<SpriteRenderer>();
            var groundSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Objects/Village/Ground.png");
            if (groundSprite != null)
            {
                groundSR.sprite = groundSprite;
                groundSR.color = Color.white;
                groundSR.sortingOrder = -1;
            }
            groundObj.transform.localScale = new Vector3(233.3333f, 5f, 1);

            // ==================== 10. 创建建筑 ====================
            string buildingsDir = "Assets/Objects/Village";
            EnsureFolder(buildingsDir);

            var buildingDefs = new[]
            {
                new BuildingDef("战斗中心", BuildingType.CombatCenter, "战斗中心.png", new Vector3(-34.1f, -3.7f, 0),
                    "Panel_Combat", typeof(CombatPanelUI)),
                new BuildingDef("食堂", BuildingType.Canteen, "食堂.png", new Vector3(-16.6f, -4.4167f, 0),
                    "Panel_Canteen", typeof(CanteenPanelUI)),
                new BuildingDef("渔场", BuildingType.Fishing, "渔场.png", new Vector3(0f, -5.1667f, 0),
                    "Panel_Fishing", typeof(FishingPanelUI)),
                new BuildingDef("研究所", BuildingType.ResearchLab, "研究所.png", new Vector3(16.5667f, -3.9833f, 0),
                    "Panel_Research", typeof(ResearchPanelUI)),
                new BuildingDef("豆网", BuildingType.Shop, "豆网.png", new Vector3(28.6f, -6.4333f, 0),
                    "Panel_Shop", typeof(ShopPanelUI)),
                new BuildingDef("村中心", BuildingType.VillageCenter, "村中心.png", new Vector3(-51.5167f, -3.1667f, 1f),
                    null, null),
            };

            foreach (var def in buildingDefs)
            {
                CreateBuilding(mgrObj.transform, panelRoot.transform, def, buildingsDir, vm);
            }

            // ==================== 11. 保存场景 ====================
            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"✅ 村庄场景搭建完成!\n场景路径: {scenePath}\n" +
                $"建筑数量: {buildingDefs.Length}\n" +
                $"PanelRoot 已置入 {panelRoot.transform.childCount} 个建筑面板\n\n" +
                "请在 Build Settings 中将 VillageScene 加入 Scenes in Build (第0位)");

            EditorUtility.DisplayDialog("搭建完成",
                $"✅ 村庄场景已创建!\n\n" +
                $"建筑: {buildingDefs.Length} 栋\n" +
                $"面板: {panelRoot.transform.childCount} 个\n\n" +
                $"接下来:\n" +
                $"1. 在 Hierarchy 中点开 VillageManagers 查看建筑\n" +
                $"2. 在 Project 中 Assets/Objects/Village/ 查看 Sprite\n" +
                $"3. 点击 Play 测试",
                "好的");
        }

        // ==================== 辅助方法 ====================

        private struct BuildingDef
        {
            public string name;
            public BuildingType type;
            public string spriteFile;
            public Vector3 position;
            public string panelName;
            public System.Type panelType;

            public BuildingDef(string name, BuildingType type, string spriteFile, Vector3 pos,
                string panelName, System.Type panelType)
            {
                this.name = name;
                this.type = type;
                this.spriteFile = spriteFile;
                this.position = pos;
                this.panelName = panelName;
                this.panelType = panelType;
            }
        }

        private static GameObject CreateGameObject(string name, params System.Type[] components)
        {
            var obj = new GameObject(name);
            foreach (var t in components)
                obj.AddComponent(t);
            return obj;
        }

        private static GameObject CreateUIObject(string name, Transform parent,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Vector2 anchoredPos)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent);
            var rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.sizeDelta = sizeDelta;
            rt.anchoredPosition = anchoredPos;
            return obj;
        }

        private static GameObject CreateResourceText(Transform parent, string objName,
            string textObjName, Vector2 pos, string text, TextAnchor anchor)
        {
            var obj = new GameObject(objName);
            obj.transform.SetParent(parent);
            var rt = obj.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(180, 40);

            var textObj = new GameObject(textObjName);
            textObj.transform.SetParent(obj.transform);
            var textRt = textObj.AddComponent<RectTransform>();
            textRt.anchoredPosition = Vector2.zero;
            textRt.sizeDelta = new Vector2(180, 40);
            var txt = textObj.AddComponent<Text>();
            txt.text = text;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 22;
            txt.alignment = anchor;
            txt.color = Color.white;
            return textObj;
        }

        private static GameObject CreateToolbarButton(Transform parent, string name,
            string label, float xPos)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent);
            var rt = obj.AddComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(xPos, 0);
            rt.sizeDelta = new Vector2(110, 50);
            var btn = obj.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = new Color(1, 1, 1, 0.8f);
            colors.highlightedColor = Color.white;
            btn.colors = colors;

            var textObj = new GameObject("Label");
            textObj.transform.SetParent(obj.transform);
            var textRt = textObj.AddComponent<RectTransform>();
            textRt.anchoredPosition = Vector2.zero;
            textRt.sizeDelta = new Vector2(110, 50);
            var txt = textObj.AddComponent<Text>();
            txt.text = label;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 20;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;

            return obj;
        }

        private static void CreateBuilding(Transform parent, Transform panelRoot,
            BuildingDef def, string assetsDir, VillageManager vm)
        {
            // --- 建筑 GameObject ---
            var buildingObj = new GameObject(def.name);
            buildingObj.transform.SetParent(parent);
            buildingObj.transform.position = def.position;

            // SpriteRenderer
            var sr = buildingObj.AddComponent<SpriteRenderer>();
            string spritePath = $"{assetsDir}/{def.spriteFile}";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sprite != null)
            {
                sr.sprite = sprite;
                sr.sortingOrder = def.type == BuildingType.VillageCenter ? 0 : 2;
            }

            // Collider
            var bc = buildingObj.AddComponent<BoxCollider2D>();
            if (sprite != null)
            {
                float w = sprite.rect.width / sprite.pixelsPerUnit;
                float h = sprite.rect.height / sprite.pixelsPerUnit;
                bc.size = new Vector2(w, h);
            }

            // VillageBuilding
            var vb = buildingObj.AddComponent<VillageBuilding>();

            // 创建 BuildingDataSO
            string soPath = $"{assetsDir}/BuildingData_{def.type}.asset";
            var so = ScriptableObject.CreateInstance<BuildingDataSO>();
            so.buildingName = def.name;
            so.buildingType = def.type;
            so.icon = sprite;
            so.levelSprites = new Sprite[] { sprite };
            so.upgradeCosts = new int[] { 0, 100, 300 };
            so.upgradeLevelRequirements = new int[] { 1, 2, 3 };
            so.unlockDescriptions = new string[] { "已解锁", "外观升级", "功能强化" };
            so.worldPosition = def.position;
            AssetDatabase.CreateAsset(so, soPath);

            SetPrivateField(vb, "_data", so);
            SetPrivateField(vb, "_spriteRenderer", sr);
            SetPrivateField(vb, "_currentLevel", 1);
            SetPrivateField(vb, "_panelName", def.panelName);

            // --- 创建对应的 UI 面板 ---
            if (def.panelName != null && def.panelType != null)
            {
                var panelObj = CreateUIObject(def.panelName, panelRoot,
                    new Vector2(0.3f, 0.2f), new Vector2(0.7f, 0.8f),
                    new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
                var panelImg = panelObj.AddComponent<Image>();
                panelImg.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

                // 标题
                var titleObj = CreateUIObject("Title", panelObj.transform,
                    new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                    new Vector2(400, 50), new Vector2(0, -40));
                var titleTxt = titleObj.AddComponent<Text>();
                titleTxt.text = def.name;
                titleTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                titleTxt.fontSize = 28;
                titleTxt.alignment = TextAnchor.MiddleCenter;
                titleTxt.color = Color.white;

                // 关闭按钮
                var closeBtnObj = CreateUIObject("CloseBtn", panelObj.transform,
                    new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
                    new Vector2(80, 40), new Vector2(-10, -35));
                var closeBtn = closeBtnObj.AddComponent<Button>();
                var closeTxt = closeBtnObj.AddComponent<Text>();
                closeTxt.text = "✕ 关闭";
                closeTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                closeTxt.fontSize = 20;
                closeTxt.alignment = TextAnchor.MiddleCenter;
                closeTxt.color = Color.white;
                closeBtn.targetGraphic = closeTxt;
                closeBtn.onClick.AddListener(() => { if (vm != null) vm.CloseAllPanels(); });

                // 添加面板组件
                var panelComp = panelObj.AddComponent(def.panelType) as BuildingPanelUI;
                SetPrivateField(panelComp, "_panelObject", panelObj);

                // 针对战斗中心特殊处理：添加进入战斗按钮
                if (def.type == BuildingType.CombatCenter)
                {
                    var pvpBtn = CreateSimpleButton(panelObj.transform, "Btn_PvP", "⚔ 开始匹配", new Vector2(0, 20));
                    var pveBtn = CreateSimpleButton(panelObj.transform, "Btn_PvE", "🗺 冒险模式", new Vector2(0, -40));

                    SetPrivateField(panelComp, "_pvpBtn", pvpBtn.GetComponent<Button>());
                    SetPrivateField(panelComp, "_pveBtn", pveBtn.GetComponent<Button>());
                }

                // 商店面板特殊处理
                if (def.type == BuildingType.Shop)
                {
                    var shopLabel = CreateUIObject("ShopContent", panelObj.transform,
                        new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                        new Vector2(400, 200), new Vector2(0, 0));
                    var shopTxt = shopLabel.AddComponent<Text>();
                    shopTxt.text = "🏪 商店功能开发中...";
                    shopTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    shopTxt.fontSize = 24;
                    shopTxt.alignment = TextAnchor.MiddleCenter;
                    shopTxt.color = Color.gray;
                }
            }

            // 面板打开逻辑已移至 VillageBuilding.Start()
        }

        private static GameObject CreateSimpleButton(Transform parent, string name,
            string label, Vector2 pos)
        {
            var obj = CreateUIObject(name, parent,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(200, 50), pos);
            var btn = obj.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = new Color(0.3f, 0.5f, 0.8f);
            colors.highlightedColor = new Color(0.4f, 0.6f, 1f);
            btn.colors = colors;
            var txt = obj.AddComponent<Text>();
            txt.text = label;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 22;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            btn.targetGraphic = txt;
            return obj;
        }

        private static Vector4 GetVisualPadding(BuildingType type)
        {
            return type switch
            {
                BuildingType.CombatCenter  => new Vector4(0, 122, 0, 123),
                BuildingType.Canteen       => new Vector4(0, 15, 0, 64),
                BuildingType.Fishing       => new Vector4(4, 60, 4, 60),
                BuildingType.ResearchLab   => new Vector4(2, 89, 5, 75),
                BuildingType.Shop          => new Vector4(273, 136, 336, 223),
                BuildingType.VillageCenter => new Vector4(3, 40, 5, 158),
                _ => Vector4.zero
            };
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
                field.SetValue(target, value);
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = Path.GetDirectoryName(path).Replace("\\", "/");
                string folderName = Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }

        private static void EnsureSpritesAreSprite()
        {
            string folder = "Assets/Objects/Village";
            if (!AssetDatabase.IsValidFolder(folder))
            {
                string parent = Path.GetDirectoryName(folder).Replace("\\", "/");
                string folderName = Path.GetFileName(folder);
                AssetDatabase.CreateFolder(parent, folderName);
            }

            // 确保地面贴图存在 (1x1 白色像素图，PPU=1 方便用 Scale 控制世界大小)
            string groundPath = $"{folder}/Ground.png";
            if (!File.Exists(groundPath))
            {
                var tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                File.WriteAllBytes(groundPath, tex.EncodeToPNG());
                AssetDatabase.ImportAsset(groundPath);

                var gi = AssetImporter.GetAtPath(groundPath) as TextureImporter;
                if (gi != null)
                {
                    gi.textureType = TextureImporterType.Sprite;
                    gi.spriteImportMode = SpriteImportMode.Single;
                    gi.spritePixelsPerUnit = 1f;
                    gi.SaveAndReimport();
                }
                Debug.Log("[VillageScene] 已创建地面贴图 Ground.png");
            }

            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
            bool anyChanged = false;
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(".png")) continue;
                if (Path.GetFileName(path) == "Ground.png") continue;

                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    bool dirty = false;
                    if (importer.textureType != TextureImporterType.Sprite)
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        importer.spriteImportMode = SpriteImportMode.Single;
                        dirty = true;
                    }
                    if (importer.spritePixelsPerUnit != 60f)
                    {
                        importer.spritePixelsPerUnit = 60f;
                        dirty = true;
                    }
                    if (dirty)
                    {
                        importer.SaveAndReimport();
                        anyChanged = true;
                        Debug.Log($"[VillageScene] 已转换: {path} (PPU=60)");
                    }
                }
            }
            if (!anyChanged)
                Debug.Log("[VillageScene] 精灵图已就绪，无需转换");
        }
    }
}
