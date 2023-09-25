using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Object = StardewValley.Object;

namespace Fishnets
{
    public interface IQualityBaitApi
    {
        int GetQuality(int currentQuality, int baitQuality);
    }

    public interface IAlternativeTexturesApi
    {
        Texture2D GetTextureForObject(Object obj, out Rectangle sourceRect);
    }

    public interface IJsonAssetsApi
    {
        int GetObjectId(string name);
    }

    public interface IDynamicGameAssetsApi
    {
        string GetDGAItemId(object item);
        object SpawnDGAItem(string fullId);
    }

    public interface ISaveAnywhereApi
    {
        void addBeforeSaveEvent(string id, Action beforeSave);
        void addAfterLoadEvent(string id, Action afterLoad);
        void addAfterSaveEvent(string id, Action afterSave);
    }
}
