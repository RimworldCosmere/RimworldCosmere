using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace FloatSubMenus {

#if VERSION_1_3
    public enum HorizontalJustification { Left, Right }
#endif

    public class FloatSubMenu : FloatMenuOption {
        private const string VUIE_ID = "vanillaexpanded.ui";
        private const string Achtung_ID = "brrainz.achtung";

        private static bool ModActive(string id) 
            => LoadedModManager.RunningMods.Any(x => x.PackageId == id);

        private static readonly bool VUIE    = ModActive(VUIE_ID);
        private static readonly bool Achtung = ModActive(Achtung_ID);

        private static readonly bool Compat =
#if VERSION_1_3
            VUIE;
#else
            false;
#endif
        private static readonly bool CompatMMM = Compat || Achtung;

        private readonly List<FloatMenuOption> subOptions;
        private readonly float extraPartWidthOuter;
        private readonly Func<Rect, bool>? extraPartOnGUIOuter;

        private FloatSubMenuInner? subMenu = null;
        private FloatMenuFilter? filter = null;
        private Action? parentCloseCallback = null;
        private bool parentSetUp = false;
        private bool subMenuOptionChosen = false;
        private bool subOptionsInitialized = false;
        private Rect extraGUIRect = new Rect(-1f, -1f, 0f, 0f);

        private static readonly Vector2 MenuOffset = new Vector2(-1f, 0f);
        //private static readonly Texture2D ArrowIcon = ContentFinder<Texture2D>.Get("Arrow");
        private const float ArrowExtraWidth = 16f;
        private const float ArrowOffset = 4f;
        private const float ArrowAlpha = 0.6f;

        private static Action CompatSub(List<FloatMenuOption> subOption)
            => () => subOption.OpenMenu();


        /// <summary>
        /// Creates a new sub-menu using the constructor with the same arguments, 
        /// unless any mods known to be incompatible are detected. In that case 
        /// creates a normal menu option that opens the sub-menu as a new menu when 
        /// clicked.
        /// </summary>
        /// 
        /// Currently this applies to: 
        ///  - Vanilla UI Expanded with RimWorld 1.3
        public static FloatMenuOption CompatCreate(string label,
                                                   List<FloatMenuOption> subOptions,
                                                   MenuOptionPriority priority = MenuOptionPriority.Default,
                                                   Thing? revalidateClickTarget = null,
                                                   float extraPartWidth = 0,
                                                   Func<Rect, bool>? extraPartOnGUI = null,
                                                   WorldObject? revalidateWorldClickTarget = null,
                                                   bool playSelectionSound = true,
                                                   int orderInPriority = 0) {
            if (Compat) {
                return new FloatMenuOption(
                    label: label,
                    action: CompatSub(subOptions),
                    priority: priority,
                    mouseoverGuiAction: null,
                    revalidateClickTarget: revalidateClickTarget,
                    extraPartWidth: extraPartWidth,
                    extraPartOnGUI: extraPartOnGUI,
                    revalidateWorldClickTarget: revalidateWorldClickTarget,
                    playSelectionSound: playSelectionSound,
                    orderInPriority: orderInPriority);
            } else {
                return new FloatSubMenu(
                    label,
                    subOptions,
                    priority,
                    revalidateClickTarget,
                    extraPartWidth,
                    extraPartOnGUI,
                    revalidateWorldClickTarget,
                    playSelectionSound,
                    orderInPriority);
            }
        }

        /// <summary>
        /// Creates a new sub-menu using the constructor with the same arguments, 
        /// unless any mods known to be incompatible with sub-menues in the menu 
        /// created by FloatMenuMakerMap are detected. In that case creates a normal 
        /// menu option that opens the sub-menu as a new menu when clicked.
        /// </summary>
        /// 
        /// Currently this applies to: 
        ///  - Achtung!
        ///  - Vanilla UI Expanded with RimWorld 1.3
        public static FloatMenuOption CompatMMMCreate(string label,
                                                      List<FloatMenuOption> subOptions,
                                                      MenuOptionPriority priority = MenuOptionPriority.Default,
                                                      Thing? revalidateClickTarget = null,
                                                      float extraPartWidth = 0,
                                                      Func<Rect, bool>? extraPartOnGUI = null,
                                                      WorldObject? revalidateWorldClickTarget = null,
                                                      bool playSelectionSound = true,
                                                      int orderInPriority = 0) {
            if (CompatMMM) {
                return new FloatMenuOption(
                    label: label,
                    action: CompatSub(subOptions),
                    priority: priority,
                    mouseoverGuiAction: null,
                    revalidateClickTarget: revalidateClickTarget,
                    extraPartWidth: extraPartWidth,
                    extraPartOnGUI: extraPartOnGUI,
                    revalidateWorldClickTarget: revalidateWorldClickTarget,
                    playSelectionSound: playSelectionSound,
                    orderInPriority: orderInPriority);
            } else {
                return new FloatSubMenu(
                    label,
                    subOptions,
                    priority,
                    revalidateClickTarget,
                    extraPartWidth,
                    extraPartOnGUI,
                    revalidateWorldClickTarget,
                    playSelectionSound,
                    orderInPriority);
            }
        }

        public FloatSubMenu(string label,
                            List<FloatMenuOption> subOptions,
                            MenuOptionPriority priority = MenuOptionPriority.Default,
                            Thing? revalidateClickTarget = null,
                            float extraPartWidth = 0,
                            Func<Rect, bool>? extraPartOnGUI = null,
                            WorldObject? revalidateWorldClickTarget = null,
                            bool playSelectionSound = true,
                            int orderInPriority = 0)
            : base(label: label,
                   action: NoAction,
                   priority: priority,
                   mouseoverGuiAction: null,
                   revalidateClickTarget: revalidateClickTarget,
                   extraPartWidth: extraPartWidth + ArrowExtraWidth,
                   extraPartOnGUI: null,
                   revalidateWorldClickTarget: revalidateWorldClickTarget,
                   playSelectionSound: playSelectionSound,
                   orderInPriority: orderInPriority) {
            this.subOptions = subOptions;
            extraPartOnGUIOuter = extraPartOnGUI;
            extraPartWidthOuter = extraPartWidth;
            this.extraPartOnGUI = DrawExtra;
        }


        /// <summary>
        /// Creates a new sub-menu using the constructor with the same arguments, 
        /// unless any mods known to be incompatible are detected. In that case 
        /// creates a normal menu option that opens the sub-menu as a new menu when 
        /// clicked.
        /// </summary>
        /// 
        /// Currently this applies to: 
        ///  - Vanilla UI Expanded with RimWorld 1.3
        public static FloatMenuOption CompatCreate(string label,
                                                   List<FloatMenuOption> subOptions,
                                                   ThingDef shownItemForIcon,
                                                   ThingStyleDef? thingStyle = null,
                                                   bool forceBasicStyle = false,
                                                   MenuOptionPriority priority = MenuOptionPriority.Default,
                                                   Thing? revalidateClickTarget = null,
                                                   float extraPartWidth = 0,
                                                   Func<Rect, bool>? extraPartOnGUI = null,
                                                   WorldObject? revalidateWorldClickTarget = null,
                                                   bool playSelectionSound = true,
                                                   int orderInPriority = 0,
                                                   int? graphicIndexOverride = null) {
            if (Compat) {
                return new FloatMenuOption(
                    label: label,
                    action: CompatSub(subOptions),
                    shownItemForIcon: shownItemForIcon,
#if !VERSION_1_3
                    thingStyle: thingStyle,
                    forceBasicStyle: forceBasicStyle,
#endif
                    priority: priority,
                    mouseoverGuiAction: null,
                    revalidateClickTarget: revalidateClickTarget,
                    extraPartWidth: extraPartWidth + ArrowExtraWidth,
                    extraPartOnGUI: null,
                    revalidateWorldClickTarget: revalidateWorldClickTarget,
                    playSelectionSound: playSelectionSound,
                    orderInPriority: orderInPriority
#if !VERSION_1_3
                    , graphicIndexOverride: graphicIndexOverride
#endif
                   );
            } else {
                return new FloatSubMenu(
                    label,
                    subOptions,
                    shownItemForIcon,
                    thingStyle,
                    forceBasicStyle,
                    priority,
                    revalidateClickTarget,
                    extraPartWidth,
                    extraPartOnGUI,
                    revalidateWorldClickTarget,
                    playSelectionSound,
                    orderInPriority,
                    graphicIndexOverride);
            }
        }

        /// <summary>
        /// Creates a new sub-menu using the constructor with the same arguments, 
        /// unless any mods known to be incompatible with sub-menues in the menu 
        /// created by FloatMenuMakerMap are detected. In that case creates a normal 
        /// menu option that opens the sub-menu as a new menu when clicked.
        /// </summary>
        /// 
        /// Currently this applies to: 
        ///  - Achtung!
        ///  - Vanilla UI Expanded with RimWorld 1.3
        public static FloatMenuOption CompatMMMCreate(string label,
                                                      List<FloatMenuOption> subOptions,
                                                      ThingDef shownItemForIcon,
                                                      ThingStyleDef? thingStyle = null,
                                                      bool forceBasicStyle = false,
                                                      MenuOptionPriority priority = MenuOptionPriority.Default,
                                                      Thing? revalidateClickTarget = null,
                                                      float extraPartWidth = 0,
                                                      Func<Rect, bool>? extraPartOnGUI = null,
                                                      WorldObject? revalidateWorldClickTarget = null,
                                                      bool playSelectionSound = true,
                                                      int orderInPriority = 0,
                                                      int? graphicIndexOverride = null) {
            if (CompatMMM) {
                return new FloatMenuOption(
                    label: label,
                    action: CompatSub(subOptions),
                    shownItemForIcon: shownItemForIcon,
#if !VERSION_1_3
                    thingStyle: thingStyle,
                    forceBasicStyle: forceBasicStyle,
#endif
                    priority: priority,
                    mouseoverGuiAction: null,
                    revalidateClickTarget: revalidateClickTarget,
                    extraPartWidth: extraPartWidth,
                    extraPartOnGUI: extraPartOnGUI,
                    revalidateWorldClickTarget: revalidateWorldClickTarget,
                    playSelectionSound: playSelectionSound,
                    orderInPriority: orderInPriority
#if !VERSION_1_3
                    , graphicIndexOverride: graphicIndexOverride
#endif
                   );
            } else {
                return new FloatSubMenu(
                    label,
                    subOptions,
                    shownItemForIcon,
                    thingStyle,
                    forceBasicStyle,
                    priority,
                    revalidateClickTarget,
                    extraPartWidth,
                    extraPartOnGUI,
                    revalidateWorldClickTarget,
                    playSelectionSound,
                    orderInPriority,
                    graphicIndexOverride);
            }
        }

        public FloatSubMenu(string label,
                            List<FloatMenuOption> subOptions,
                            ThingDef shownItemForIcon,
                            ThingStyleDef? thingStyle = null, 
                            bool forceBasicStyle = false, 
                            MenuOptionPriority priority = MenuOptionPriority.Default,
                            Thing? revalidateClickTarget = null,
                            float extraPartWidth = 0,
                            Func<Rect, bool>? extraPartOnGUI = null,
                            WorldObject? revalidateWorldClickTarget = null,
                            bool playSelectionSound = true,
                            int orderInPriority = 0, 
                            int? graphicIndexOverride = null)
            : base(label: label,
                   action: NoAction,
                   shownItemForIcon: shownItemForIcon,
#if !VERSION_1_3
                   thingStyle: thingStyle,
                   forceBasicStyle: forceBasicStyle,
#endif
                   priority: priority,
                   mouseoverGuiAction: null,
                   revalidateClickTarget: revalidateClickTarget,
                   extraPartWidth: extraPartWidth + ArrowExtraWidth,
                   extraPartOnGUI: null,
                   revalidateWorldClickTarget: revalidateWorldClickTarget,
                   playSelectionSound: playSelectionSound,
                   orderInPriority: orderInPriority
#if !VERSION_1_3
                   , graphicIndexOverride: graphicIndexOverride
#endif
                  ) {
            this.subOptions = subOptions;
            extraPartOnGUIOuter = extraPartOnGUI;
            extraPartWidthOuter = extraPartWidth;
            this.extraPartOnGUI = DrawExtra;
        }


        /// <summary>
        /// Creates a new sub-menu using the constructor with the same arguments, 
        /// unless any mods known to be incompatible are detected. In that case 
        /// creates a normal menu option that opens the sub-menu as a new menu when 
        /// clicked.
        /// </summary>
        /// 
        /// Currently this applies to: 
        ///  - Vanilla UI Expanded with RimWorld 1.3
        public static FloatMenuOption CompatCreate(string label,
                                                   List<FloatMenuOption> subOptions,
                                                   Texture2D itemIcon,
                                                   Color iconColor,
                                                   MenuOptionPriority priority = MenuOptionPriority.Default,
                                                   Thing? revalidateClickTarget = null,
                                                   float extraPartWidth = 0,
                                                   Func<Rect, bool>? extraPartOnGUI = null,
                                                   WorldObject? revalidateWorldClickTarget = null,
                                                   bool playSelectionSound = true,
                                                   int orderInPriority = 0,
                                                   HorizontalJustification iconJustification = HorizontalJustification.Left,
                                                   bool extraPartRightJustified = false) {
            if (Compat) {
                return new FloatMenuOption(
                    label: label,
                    action: CompatSub(subOptions),
                    iconTex: itemIcon,
                    iconColor: iconColor,
                    priority: priority,
                    mouseoverGuiAction: null,
                    revalidateClickTarget: revalidateClickTarget,
                    extraPartWidth: extraPartWidth,
                    extraPartOnGUI: extraPartOnGUI,
                    revalidateWorldClickTarget: revalidateWorldClickTarget,
                    playSelectionSound: playSelectionSound,
                    orderInPriority: orderInPriority
#if !VERSION_1_3
                    , iconJustification: iconJustification,
                    extraPartRightJustified: extraPartRightJustified
#endif
                  );
            } else {
                return new FloatSubMenu(
                    label,
                    subOptions,
                    itemIcon,
                    iconColor,
                    priority,
                    revalidateClickTarget,
                    extraPartWidth,
                    extraPartOnGUI,
                    revalidateWorldClickTarget,
                    playSelectionSound,
                    orderInPriority,
                    iconJustification,
                    extraPartRightJustified);
            }
        }

        /// <summary>
        /// Creates a new sub-menu using the constructor with the same arguments, 
        /// unless any mods known to be incompatible with sub-menues in the menu 
        /// created by FloatMenuMakerMap are detected. In that case creates a normal 
        /// menu option that opens the sub-menu as a new menu when clicked.
        /// </summary>
        /// 
        /// Currently this applies to: 
        ///  - Achtung!
        ///  - Vanilla UI Expanded with RimWorld 1.3
        public static FloatMenuOption CompatMMMCreate(string label,
                                                      List<FloatMenuOption> subOptions,
                                                      Texture2D itemIcon,
                                                      Color iconColor,
                                                      MenuOptionPriority priority = MenuOptionPriority.Default,
                                                      Thing? revalidateClickTarget = null,
                                                      float extraPartWidth = 0,
                                                      Func<Rect, bool>? extraPartOnGUI = null,
                                                      WorldObject? revalidateWorldClickTarget = null,
                                                      bool playSelectionSound = true,
                                                      int orderInPriority = 0,
                                                      HorizontalJustification iconJustification = HorizontalJustification.Left,
                                                      bool extraPartRightJustified = false) {
            if (CompatMMM) {
                return new FloatMenuOption(
                    label: label,
                    action: CompatSub(subOptions),
                    iconTex: itemIcon,
                    iconColor: iconColor,
                    priority: priority,
                    mouseoverGuiAction: null,
                    revalidateClickTarget: revalidateClickTarget,
                    extraPartWidth: extraPartWidth,
                    extraPartOnGUI: extraPartOnGUI,
                    revalidateWorldClickTarget: revalidateWorldClickTarget,
                    playSelectionSound: playSelectionSound,
                    orderInPriority: orderInPriority
#if !VERSION_1_3
                    , iconJustification: iconJustification,
                    extraPartRightJustified: extraPartRightJustified
#endif
                  );
            } else {
                return new FloatSubMenu(
                    label,
                    subOptions,
                    itemIcon,
                    iconColor,
                    priority,
                    revalidateClickTarget,
                    extraPartWidth,
                    extraPartOnGUI,
                    revalidateWorldClickTarget,
                    playSelectionSound,
                    orderInPriority,
                    iconJustification,
                    extraPartRightJustified);
            }
        }

        public FloatSubMenu(string label,
                            List<FloatMenuOption> subOptions,
                            Texture2D itemIcon,
                            Color iconColor,
                            MenuOptionPriority priority = MenuOptionPriority.Default,
                            Thing? revalidateClickTarget = null,
                            float extraPartWidth = 0,
                            Func<Rect, bool>? extraPartOnGUI = null,
                            WorldObject? revalidateWorldClickTarget = null,
                            bool playSelectionSound = true,
                            int orderInPriority = 0,
                            HorizontalJustification iconJustification = HorizontalJustification.Left, 
                            bool extraPartRightJustified = false)
            : base(label: label,
                   action: NoAction,
                   iconTex: itemIcon,
                   iconColor: iconColor,
                   priority: priority,
                   mouseoverGuiAction: null,
                   revalidateClickTarget: revalidateClickTarget,
                   extraPartWidth: extraPartWidth + ArrowExtraWidth,
                   extraPartOnGUI: null,
                   revalidateWorldClickTarget: revalidateWorldClickTarget,
                   playSelectionSound: playSelectionSound,
                   orderInPriority: orderInPriority
#if !VERSION_1_3
                   , iconJustification: iconJustification,
                   extraPartRightJustified: extraPartRightJustified
#endif
                  ) {
            this.subOptions = subOptions;
            extraPartOnGUIOuter = extraPartOnGUI;
            extraPartWidthOuter = extraPartWidth;
            this.extraPartOnGUI = DrawExtra;
        }

        private static void NoAction() { }

        public bool DrawExtra(Rect rect) {
            extraGUIRect = rect.RightPartPixels(ArrowExtraWidth);
            extraPartOnGUIOuter?.Invoke(rect.LeftPartPixels(extraPartWidthOuter));
            return false;
        }

        private static void DrawArrow(Rect rect) {
            rect.width -= ArrowOffset;

            GameFont font = Text.Font;
            TextAnchor anchor = Text.Anchor;
            Color color = GUI.color;

            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleRight;
            GUI.color = new Color(color.r, color.g, color.b, color.a * ArrowAlpha);
            Widgets.Label(rect, ">");

            Text.Font = font;
            Text.Anchor = anchor;
            GUI.color = color;
        }

        public override bool DoGUI(Rect rect, bool colonistOrdering, FloatMenu floatMenu) {
            if (floatMenu == null) {
                return base.DoGUI(rect, colonistOrdering, floatMenu);
            }

            SetupParent(floatMenu);

            MouseArea mouseArea = FindMouseArea(rect, floatMenu);
            bool inExtraSpace = Mouse.IsOver(extraGUIRect);
            if (mouseArea == (Open ? MouseArea.Menu : MouseArea.Option)) {
                MouseAction(rect, !Open, floatMenu);
            }

            // When the sub menu is open, let super implementation know only about
            // mouse movement inside parent menu. Also do not let it know if the
            // mouse is in our extraPartOnGUI space, since it does not highlight
            // option then.
            Vector2 mouse = Event.current.mousePosition;
            if ((Open && mouseArea == MouseArea.Outside) || inExtraSpace) {
                Event.current.mousePosition = new Vector2(rect.x + 2f, rect.y + 2f);
            }

            base.DoGUI(rect, colonistOrdering, floatMenu);
            DrawArrow(rect);

            // Reset mouse position
            Event.current.mousePosition = mouse;

            if (subMenuOptionChosen) { floatMenu.PreOptionChosen(this); }
            return subMenuOptionChosen;
        }

        public bool Open => subMenu != null && subMenu.IsOpen;

        public List<FloatMenuOption> Options {
            get {
                if (!subOptionsInitialized) {
                    FloatMenuSizeMode mode = subOptions.Count > 60 ? FloatMenuSizeMode.Tiny : FloatMenuSizeMode.Normal;
                    subOptions.ForEach(o => o.SetSizeMode(mode));
                    subOptions.Sort(OptionPriorityCmp);
                    subOptionsInitialized = true;
                }
                return subOptions;
            }
        }

        internal bool AnyMatches(Func<FloatMenuOption, bool> predicate, bool recursive) 
            => subOptions.Any(x => predicate(x) || (recursive && SubAnyMatches(x, predicate)));

        private bool SubAnyMatches(FloatMenuOption opt, Func<FloatMenuOption, bool> predicate) 
            => opt is FloatSubMenu sub && sub.AnyMatches(predicate, true);

        private FloatMenuFilter Filter => filter ?? (filter = new FloatMenuFilter());

        internal void FilterSubMenu(Func<FloatMenuOption, bool> predicate, bool reset, bool recursive)
            => Filter.Filter(predicate, reset, recursive);

        internal void UpdateFilter(FloatMenu floatMenu)
            => filter?.Update(floatMenu);

        private static int OptionPriorityCmp(FloatMenuOption a, FloatMenuOption b) {
            // Should sort decending, so flipped order
            int res = (int) b.Priority - (int) a.Priority;
            return (res != 0) ? res : b.orderInPriority - a.orderInPriority;
        }

        internal static bool ShouldReplaceDistanceFor(FloatMenu menu, ref float distance) {
            OpenMenuSet? set = OpenMenuSet.For(menu);
            if (set != null) {
                distance = set.MinDistance;
                return true;
            } else {
                return false;
            }
        }

        private void SetupParent(FloatMenu parent) {
            if (!(parentSetUp || parent is FloatSubMenuInner)) {
                parentSetUp = true;
                parentCloseCallback = parent.onCloseCallback;
                parent.onCloseCallback = OnParentClose;
            }
        }

        private void OnParentClose() {
            CloseSubMenu();
            // Call any previously set action
            parentCloseCallback?.Invoke();
        }

        private enum MouseArea { Option, Menu, Outside }

        private MouseArea FindMouseArea(Rect option, FloatMenu menu) {
            option.height--;
            if (Mouse.IsOver(option)) { return MouseArea.Option; }

            // As the current window being drawn, origo will be relative to menu
            return Mouse.IsOver(menu.windowRect.AtZero()) ? MouseArea.Menu : MouseArea.Outside;
        }

        private void MouseAction(Rect rect, bool enter, FloatMenu parentMenu) {
            if (enter) {
                Vector2 localPos = new Vector2(rect.xMax, rect.yMin) + MenuOffset;
                OpenSubMenu(parentMenu, localPos);
            } else {
                CloseSubMenu();
            }
        }

        private void OpenSubMenu(FloatMenu parentMenu, Vector2 localPos) {
            if (!Open) {
                Vector2 mouse = Event.current.mousePosition;
                Vector2 offset = localPos - mouse;
                SoundDef sound = SoundDefOf.FloatMenu_Open;
                SoundDefOf.FloatMenu_Open = null;
                subMenu = new FloatSubMenuInner(
                    this,
                    subOptions,
                    offset,
                    parentMenu.vanishIfMouseDistant);
                SoundDefOf.FloatMenu_Open = sound;
                subOptionsInitialized = true;
                Find.WindowStack.Add(subMenu);
                OpenMenuSet.Open(parentMenu, subMenu);
            }
        }

        private void CloseSubMenu() {
            if (Open) {
                Find.WindowStack.TryRemove(subMenu, doCloseSound: false);
                subMenu = null;
            }
        }

        private class OpenMenuSet {
            private static readonly Dictionary<FloatMenu,OpenMenuSet> sets =
                new Dictionary<FloatMenu, OpenMenuSet>();

            private readonly List<FloatMenu> menus = new List<FloatMenu>();
            private readonly List<Rect> rects = new List<Rect>();

            private bool cacheValid = false;
            private Vector2 cachedPosition;
            private float cachedDistance;

            private OpenMenuSet(FloatMenu parent, FloatSubMenuInner child) {
                Add(parent);
                Add(child);
            }

            private void Add(FloatMenu menu) {
                if (!menus.Contains(menu)) {
                    menus.Add(menu);
                    rects.Add(menu.windowRect.ContractedBy(-5f));
                }
                sets[menu] = this;
                cacheValid = false;
            }

            private void Remove(int i) {
                sets.Remove(menus[i]);
                menus.RemoveAt(i);
                rects.RemoveAt(i);
                cacheValid = false;
            }

            private void Remove(FloatMenu menu) {
                int i = menus.IndexOf(menu);
                if (i > 0) {
                    Remove(i);
                }
                // If only the vanilla menu is left, remove it as well.
                // If the menu to remove is the vanilla one, remove all.
                if (menus.Count == 1 || i == 0) {
                    for (int j = menus.Count - 1; j >= 0; j--) {
                        Remove(j);
                    }
                }
            }

            public static OpenMenuSet? For(FloatMenu menu) =>
                sets.TryGetValue(menu, out OpenMenuSet? set) ? set : null;

            public static void Open(FloatMenu parent, FloatSubMenuInner child) {
                if (sets.TryGetValue(parent, out OpenMenuSet? set)) {
                    set.Add(child);
                } else {
                    new OpenMenuSet(parent, child);
                }
                string list = sets[parent].menus.Select(m => m.ID).ToStringSafeEnumerable();
            }

            public static void Close(FloatMenu menu) {
                if (sets.TryGetValue(menu, out OpenMenuSet? set)) {
                    set.Remove(menu);
                }
            }

            public float MinDistance {
                get {
                    Vector2 pos = UI.MousePositionOnUIInverted;
                    if (!cacheValid || pos != cachedPosition) {
                        cacheValid = true;
                        cachedPosition = pos;
                        cachedDistance = rects.Min(r => GenUI.DistFromRect(r, pos));
                    }
                    return cachedDistance;
                }
            }
        }

        private class FloatSubMenuInner : FloatMenu {
            public Vector2 mouseOffset;
            public FloatSubMenu parent;

            public FloatSubMenuInner(FloatSubMenu parent, List<FloatMenuOption> options, Vector2 mouseOffset, bool vanish) 
                    : base(options) {
                this.mouseOffset = mouseOffset;
                this.parent = parent;
                onlyOneOfTypeAllowed = false;
                vanishIfMouseDistant = vanish;
                parent.UpdateFilter(this);
            }

            public override void DoWindowContents(Rect rect) {
                parent.UpdateFilter(this);
                base.DoWindowContents(rect);
            }

            protected override void SetInitialSizeAndPosition() {
                Vector2 pos = UI.MousePositionOnUIInverted + mouseOffset;
                Vector2 size = InitialSize;
                float x = Mathf.Min(pos.x, UI.screenWidth - size.x);
                float y = Mathf.Min(pos.y, UI.screenHeight - size.y);
                windowRect = new Rect(x, y, size.x, size.y);
            }

            public override void PreOptionChosen(FloatMenuOption opt) {
                parent.subMenuOptionChosen = true;
                base.PreOptionChosen(opt);
            }

            public override void PreClose() {
                foreach (FloatSubMenu sub in options.OfType<FloatSubMenu>()) {
                    sub.CloseSubMenu();
                }
                OpenMenuSet.Close(this);
            }
        }
    }
}
