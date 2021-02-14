using System;

namespace SpeedDatingBot.Models
{
    public interface IModelHandler<TKey, TModel>
    {
        public TModel GetItem(TKey key);
        public TModel UpdateItem(TKey key);
        public TModel DeleteItem(TKey key);
    }
}