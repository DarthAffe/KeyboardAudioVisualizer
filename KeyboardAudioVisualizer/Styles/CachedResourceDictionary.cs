using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;

namespace KeyboardAudioVisualizer.Styles
{
    public class CachedResourceDictionary : ResourceDictionary
    {
        #region Properties & Fields

        // ReSharper disable InconsistentNaming
        private static readonly List<string> _cachedDictionaries = new List<string>();
        private static readonly ResourceDictionary _innerDictionary = new ResourceDictionary();
        // ReSharper restore 

        public new Uri Source
        {
            get => null;
            set
            {
                lock (_innerDictionary)
                {
                    UpdateCache(value);

                    MergedDictionaries.Clear();
                    MergedDictionaries.Add(_innerDictionary);
                }
            }
        }

        #endregion

        #region Methods

        private static void UpdateCache(Uri source)
        {
            string uriPath = source.OriginalString;
            if (_cachedDictionaries.Contains(uriPath)) return;

            _cachedDictionaries.Add(uriPath);

            ResourceDictionary newDictionary = new ResourceDictionary { Source = new Uri(uriPath, source.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative) };
            CopyDictionaryEntries(newDictionary, _innerDictionary);
        }

        private static void CopyDictionaryEntries(IDictionary source, IDictionary target)
        {
            foreach (object key in source.Keys)
                if (!target.Contains(key))
                    target.Add(key, source[key]);
        }

        #endregion
    }
}
