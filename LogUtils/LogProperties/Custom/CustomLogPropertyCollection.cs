﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace LogUtils.Properties.Custom
{
    public class CustomLogPropertyCollection : IEnumerable<CustomLogProperty>
    {
        protected List<CustomLogProperty> InnerList = [];

        public EventDelegate OnPropertyAdded, OnPropertyRemoved;

        public void AddProperty(CustomLogProperty property)
        {
            InnerList.Add(property);
            OnPropertyAdded?.Invoke(property);
        }

        public bool RemoveProperty(CustomLogProperty property)
        {
            if (InnerList.Remove(property))
            {
                OnPropertyRemoved?.Invoke(property);
                return true;
            }
            return false;
        }

        public bool RemoveProperty(Predicate<CustomLogProperty> match)
        {
            int propertyIndex = InnerList.FindIndex(match);

            if (propertyIndex != -1)
            {
                CustomLogProperty property = InnerList[propertyIndex];

                InnerList.RemoveAt(propertyIndex);
                OnPropertyRemoved?.Invoke(property);
                return true;
            }
            return false;
        }

        public IEnumerator<CustomLogProperty> GetEnumerator()
        {
            return InnerList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return InnerList.GetEnumerator();
        }

        public delegate void EventDelegate(CustomLogProperty property);
    }
}
