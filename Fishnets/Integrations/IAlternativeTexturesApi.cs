using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fishnets.Integrations
{
    public interface IAlternativeTexturesApi
    {
        Texture2D GetTextureForObject(Object obj, out Rectangle sourceRect);
    }
}
