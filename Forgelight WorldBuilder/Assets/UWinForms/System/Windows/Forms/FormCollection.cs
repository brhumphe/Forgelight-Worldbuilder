﻿namespace UWinForms.System.Windows.Forms
{
    using global::System;
    using global::System.Collections;
    using global::System.Collections.Generic;

    internal sealed class FormCollection : ICollection
    {
        private readonly List<Form> items = new List<Form>();
        private object syncRoot;

        public int Count { get { return items.Count; } }

        bool ICollection.IsSynchronized { get { return false; } }
        object ICollection.SyncRoot
        {
            get
            {
                if (syncRoot == null)
                    syncRoot = new object();
                return syncRoot;
            }
        }

        public Form this[int index]
        {
            get { return items[index]; }
        }

        public void Add(Form form)
        {
            items.Add(form);

            Sort();
        }
        public bool Contains(Form form)
        {
            return items.Contains(form);
        }
        public void Remove(Form form)
        {
            items.Remove(form);
        }

        public IEnumerator GetEnumerator()
        {
            return items.GetEnumerator();
        }
        public void CopyTo(Array array, int index)
        {
            items.ToArray().CopyTo(array, index);
        }

        internal void Sort()
        {
            bool topMostForms = true;
            int lastTopMost = -1;
            for (int i = items.Count - 1; i >= 0; i--)
            {
                var form = items[i];
                if (topMostForms)
                {
                    if (form.TopMost == false)
                        topMostForms = false;
                    else
                        lastTopMost = i;
                }
                else
                {
                    if (form.TopMost)
                    {
                        if (lastTopMost != -1)
                        {
                            items.Insert(lastTopMost, form);
                            lastTopMost--;
                        }
                        else
                        {
                            lastTopMost = items.Count - 1;
                            items.Add(form);
                        }
                        items.RemoveAt(i);
                    }
                }
            }
        }
    }
}
