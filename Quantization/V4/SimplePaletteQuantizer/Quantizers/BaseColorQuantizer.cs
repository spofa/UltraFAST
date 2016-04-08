using System;
using System.Drawing;
using System.Collections.Generic;
using SimplePaletteQuantizer.ColorCaches;

namespace SimplePaletteQuantizer.Quantizers
{
    public abstract class BaseColorQuantizer : IColorQuantizer
    {
        #region | Constants |

        private const int InvalidPaletteIndex = -1;

        #endregion

        #region | Fields |

        private IColorCache cache;

        #endregion

        #region | Constructors |

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseColorQuantizer"/> class.
        /// </summary>
        protected BaseColorQuantizer()
        {
            cache = null;
        }

        #endregion

        #region | Methods |

        /// <summary>
        /// Changes the cache provider.
        /// </summary>
        /// <param name="colorCache">The color cache.</param>
        public void ChangeCacheProvider(IColorCache colorCache)
        {
            cache = colorCache;
        }

        /// <summary>
        /// Caches the palette.
        /// </summary>
        /// <param name="palette">The palette.</param>
        public void CachePalette(IList<Color> palette)
        {
            IColorCache colorCache = GetColorCache();
            colorCache.CachePalette(palette);
        }

        #endregion

        #region | Helper methods |

        private IColorCache GetColorCache()
        {
            // if there is no cache, it attempts to create a default cache; integrated in the quantizer
            return cache ?? (cache = OnCreateDefaultCache());
        }

        #endregion

        #region | Abstract/virtual methods |

        /// <summary>
        /// Called to retrieve current palette.
        /// </summary>
        /// <param name="colorCount">The color count.</param>
        /// <returns>The current palette.</returns>
        protected abstract List<Color> OnGetPalette(Int32 colorCount);

        /// <summary>
        /// Called when it is needed to create default cache (no cache is supplied from outside).
        /// </summary>
        /// <returns></returns>
        protected abstract IColorCache OnCreateDefaultCache();

        /// <summary>
        /// Called when get palette index for a given color should be returned.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="paletteIndex">Index of the palette.</param>
        protected virtual void OnGetPaletteIndex(Color color, out Int32 paletteIndex)
        {
            paletteIndex = InvalidPaletteIndex;
        }

        #endregion

        #region << IColorQuantizer >>

        /// <summary>
        /// See <see cref="IColorQuantizer.Prepare"/> for more details.
        /// </summary>
        public virtual void Prepare(Image image)
        {
            IColorCache colorCache = GetColorCache();
            colorCache.Prepare();
        }

        /// <summary>
        /// See <see cref="IColorQuantizer.AddColor"/> for more details.
        /// </summary>
        public abstract void AddColor(Color color);

        /// <summary>
        /// See <see cref="IColorQuantizer.GetColorCount"/> for more details.
        /// </summary>
        public abstract Int32 GetColorCount();

        /// <summary>
        /// See <see cref="IColorQuantizer.GetPalette"/> for more details.
        /// </summary>
        public List<Color> GetPalette(Int32 colorCount)
        {
            List<Color> palette = OnGetPalette(colorCount);
            IColorCache colorCache = GetColorCache();
            if (colorCache != null) colorCache.CachePalette(palette);
            return palette;
        }

        /// <summary>
        /// See <see cref="IColorQuantizer.GetPaletteIndex"/> for more details.
        /// </summary>
        public Int32 GetPaletteIndex(Color color)
        {
            Int32 result = InvalidPaletteIndex;

            if (cache == null)
            {
                // first tries to use native quantizer resources
                OnGetPaletteIndex(color, out result);
            }

            // if no palette index is returned; then try to use cache
            if (result == InvalidPaletteIndex)
            {
                IColorCache colorCache = GetColorCache();

                // if the cache exists; or default one was created for these purposes.. use it
                if (colorCache != null)
                {
                    colorCache.GetColorPaletteIndex(color, out result);
                }
                else // otherwise there's no way to determine palette index; sorry no cigar
                {
                    String message = string.Format("The color cache is not initialized! Please use SetColorCache() method on quantizer.");
                    throw new ArgumentNullException(message);
                }
            }
            
            return result;
        }

        #endregion
    }
}
