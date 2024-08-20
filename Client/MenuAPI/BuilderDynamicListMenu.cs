using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using ScaleformUI.Menu;

namespace test_project.Client.MenuAPI
{
    public class BuilderDynamicListMenu<T> : BuilderBaseListMenu<T>
    {
        public delegate Task<T[]> PopulateItemsAction(object sender);
        private readonly PopulateItemsAction _populateAction;

        public BuilderDynamicListMenu(string title, string header, string description, PointF offset, PopulateItemsAction populateItemAction) : base(title, header, description,
            offset)
        {
            _populateAction = populateItemAction;

            Menu.OnMenuClose += menu =>
            {
                Items.Clear();
            };
        }

        protected override async Task<List<T>> PopulateItems()
        {
            T[] result = await _populateAction(this);
            return result.ToList();
        }
    }
}