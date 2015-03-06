using iTextSharp.text;
using iTextSharp.text.pdf.parser;

namespace iTextSharp.javaone.edition14.part4.helper
{
    /// <summary>
    /// Subclass of the MyItem class that is used to store the coordinates
    /// of an image.
    /// </summary>
    public class ImageItem : MyItem
    {
        /// <summary>
        /// Images will be marked in this color.
        /// </summary>
        public static readonly BaseColor IMG_COLOR = BaseColor.RED;

        /// <summary>
        /// Creates an ImageItem based on an ImageRenderInfo object.
        /// </summary>
        /// <param name="imageRenderInfo">Object containing info about an image</param>
        public ImageItem(ImageRenderInfo imageRenderInfo) : base()
        {
            rectangle = GetRectangle(imageRenderInfo);
            color = IMG_COLOR;
        }

        /// <summary>
        /// Converts the Matrix containing the coordinates of an image as stored
        /// in an ImageRenderInfo object into a Rectangle.
        /// </summary>
        /// <param name="imageRenderInfo">Object that contains info about an image</param>
        /// <returns>coordinates in the form of a Rectangle object</returns>
        private static Rectangle GetRectangle(ImageRenderInfo imageRenderInfo)
        {
            Matrix ctm = imageRenderInfo.GetImageCTM();
            return new Rectangle(ctm[6], ctm[7], ctm[6] + ctm[0], ctm[7] + ctm[4]);
        }
    }
}
