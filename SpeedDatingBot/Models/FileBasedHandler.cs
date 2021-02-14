namespace SpeedDatingBot.Models
{
    public class FileBasedModelHandler<TKey, TModel> : IModelHandler<TKey, TModel>
    {
        public TModel GetItem(TKey key)
        {
            throw new System.NotImplementedException();
        }

        public TModel UpdateItem(TKey key)
        {
            throw new System.NotImplementedException();
        }

        public TModel DeleteItem(TKey key)
        {
            throw new System.NotImplementedException();
        }
    }
}