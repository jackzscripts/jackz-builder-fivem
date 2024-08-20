using System;
using System.Collections.Generic;
using System.Drawing;
using CitizenFX.Core;
using ScaleformUI.Menu;

namespace test_project.Client.MenuAPI
{
    /// <summary>
    /// Creates a list menu from a predefined list. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BuilderListMenu<T> : BuilderBaseListMenu<T>
    {
        public BuilderListMenu(List<T> items, string title, string header, string description, PointF offset) : base(title, header, description,
            offset)
        {
            Items = items;
        }

        public void AddItem(T item)
        {
            Items.Add(item);
        }

        public void RemoveItem(T item)
        {
            Items.Remove(item);
        }

        public void RemoveItem(int index)
        {
            Items.RemoveAt(index);
        }

        public T GetItem(int index)
        {
            return Items[index];
        }

        public bool HasItem(T item)
        {
            return Items.Contains(item);
        }

    }
    
}