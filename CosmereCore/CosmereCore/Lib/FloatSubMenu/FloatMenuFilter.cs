using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FloatSubMenus {
    internal class FloatMenuFilter {
        private List<FloatMenuOption> options;
        private List<FloatMenuOption> filtered;
        private FloatMenuSizeMode sizeMode = FloatMenuSizeMode.Undefined;
        private FloatMenu initialized;
        private bool updateSize;
        private (Func<FloatMenuOption, bool> predicate, bool reset, bool recursive) delayed;

        public IEnumerable<FloatMenuOption> Unfiltered => options;
        public IEnumerable<FloatMenuOption> Filtered => filtered;
        public int Count => filtered.Count;

        public void Filter(Func<FloatMenuOption, bool> predicate,
                           bool reset = false,
                           bool recursive = false) {
            if (initialized == null) {
                delayed = (predicate, reset, recursive);
                return;
            }

            filtered.Clear();
            foreach (var option in options) {
                var sub = recursive ? option as FloatSubMenu : null;
                bool match = reset || predicate(option);
                if (match || (sub?.AnyMatches(predicate, recursive) ?? false)) {
                    filtered.Add(option);
                    sub?.FilterSubMenu(predicate, match, recursive);
                }
            }
            updateSize = true;
        }

        public void Update(FloatMenu floatMenu, Action onInit = null, Action onResize = null) {
            if (initialized != floatMenu) Init(floatMenu, onInit);
            if (updateSize) UpdateSize(floatMenu, onResize);
        }

        protected void Init(FloatMenu floatMenu, Action action) {
            var listField = Traverse.Create(floatMenu).Field<List<FloatMenuOption>>("options");
            options = listField.Value;
            listField.Value = filtered = options.ToList();
            initialized = floatMenu;
            action?.Invoke();
            if (delayed.predicate != null) {
                Filter(delayed.predicate, delayed.reset, delayed.recursive);
                delayed.predicate = null;
            }
        }

        protected void UpdateSize(FloatMenu floatMenu, Action action) {
            var mode = floatMenu.SizeMode;
            if (sizeMode != mode) {
                options.ForEach(x => x.SetSizeMode(mode));
                sizeMode = mode;
            }

            floatMenu.windowRect.size = floatMenu.InitialSize;
            floatMenu.windowRect.xMax = Mathf.Min(floatMenu.windowRect.xMax, UI.screenWidth);
            floatMenu.windowRect.yMax = Mathf.Min(floatMenu.windowRect.yMax, UI.screenHeight);

            updateSize = false;
            action?.Invoke();
        }
    }
}
