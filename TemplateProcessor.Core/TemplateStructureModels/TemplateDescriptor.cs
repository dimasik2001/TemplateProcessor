using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TemplateProcessor.Core.TemplateStructureModels
{
    public class TemplateDescriptor
    {
        private const string indexCollectionPostfix = "_index";
        public Dictionary<string, object> SingleTemplateEntities { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, IEnumerable<object>> CollectionTemplateEntities { get; set; } = new Dictionary<string, IEnumerable<object>>();
        public void AddIndexesForAllCollections(Func<int, object> indexConverter = null)
        {
            foreach (var collectionKey in CollectionTemplateEntities.Keys.Where(key => !key.EndsWith(indexCollectionPostfix)).ToArray())
            {
                AddIndexForCollectionByKey(collectionKey, indexConverter);
            }
        }
        public void AddIndexForCollectionByKey(string collectionKey, Func<int, object> indexConverter = null)
        {
            var collection = CollectionTemplateEntities[collectionKey];
            CollectionTemplateEntities[collectionKey + indexCollectionPostfix] = indexConverter == null ? GetIndexCollection(collection) : GetIndexCollection(collection, indexConverter);
        }

        private static IEnumerable<object> GetIndexCollection<T>(IEnumerable<T> collection, Func<int, object> indexConverter)
        {
            var index = 0;
            foreach (var item in collection)
            {
                yield return indexConverter?.Invoke(index++);
            }
        }
        private static IEnumerable<object> GetIndexCollection<T>(IEnumerable<T> collection)
        {
            var index = 0;
            foreach (var item in collection)
            {
                yield return index++;
            }
        }
    }
}
