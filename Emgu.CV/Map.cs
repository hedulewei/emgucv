using System;
using System.Collections.Generic;
using System.Text;
using Emgu.CV;

namespace Emgu.CV
{
    /// <summary>
    /// A Map is similar to an Image, except that the location of the pixels is defined by 
    /// its area and resolution
    /// </summary>
    /// <typeparam name="TColor">The color of this map</typeparam>
    /// <typeparam name="TDepth">The depth of this map</typeparam>
    public class Map<TColor, TDepth> : Image<TColor, TDepth>  where TColor : ColorType, new() 
    {
        private Rectangle<double> _area;

        /// <summary>
        /// Create a new Image Map defined by the Rectangle area. The center (0.0, 0.0) of this map is 
        /// defined by the center of the rectangle.
        /// </summary>
        /// <param name="area"></param>
        /// <param name="resolution">The resolution of x (y), (e.g. a value of 0.5 means each cell in the map is 0.5 unit in x (y) dimension)</param>
        /// <param name="color"> The initial color of the map</param>
        public Map(Rectangle<double> area, Point2D<double> resolution, TColor color)
             : base(
                System.Convert.ToInt32((area.Width) / resolution.X),
                System.Convert.ToInt32((area.Height) / resolution.Y),
                color)
        { 
            _area = area;
        }

        /// <summary>
        /// Create a new Image Map defined by the Rectangle area. The center (0.0, 0.0) of this map is 
        /// defined by the center of the rectangle. The initial value of the map is 0.0
        /// </summary>
        /// <param name="area"></param>
        /// <param name="resolution">The resolution of x (y), (e.g. a value of 0.5 means each cell in the map is 0.5 unit in x (y) dimension)</param>
        public Map(Rectangle<double> area, Point2D<double> resolution)
            : this(area, resolution, new TColor())
        {
        }

        /// <summary>
        /// Create a new Map using the specific image and the rectangle area
        /// </summary>
        /// <param name="image">The image of this map</param>
        /// <param name="area">The area of this map</param>
        public Map(Image<TColor, TDepth> image, Rectangle<double> area)
            : base(image.Width, image.Height)
        {
            image.Copy(this);
            _area = area;
        }
    
        /// <summary>
        /// The area of this map as a rectangle
        /// </summary>
        public Rectangle<double> Area
        {
            get { return _area; }
        }

        /// <summary>
        /// The resolution of this map as a 2D point
        /// </summary>
        public Point2D<double> Resolution
        {
            get { return new Point2D<double>(Area.Width / Width, Area.Height / Height); }
        }

        /// <summary>
        /// Map a point to a position in the internal image
        /// </summary>
        /// <typeparam name="D2"></typeparam>
        /// <param name="pt"></param>
        /// <returns></returns>
        private Point2D<double> MapPoint<D2>( Point2D<D2> pt) where D2: IComparable, new()
        {
            return new Point2D<double>(
                (System.Convert.ToDouble(pt.X) - System.Convert.ToDouble(Area.Left)) / Resolution.X,
                (System.Convert.ToDouble(pt.Y) - System.Convert.ToDouble(Area.Bottom)) / Resolution.Y);
        }

        /// <summary>
        /// Draw a rectangle in the map
        /// </summary>
        /// <typeparam name="T">The type of the rectangle</typeparam>
        /// <param name="rect">The rectangle to draw</param>
        /// <param name="color">The color for the rectangle</param>
        /// <param name="thickness">The thickness of the rectangle, any value less than or equal to 0 will result in a filled rectangle</param>
        public override void Draw<T>(Rectangle<T> rect, TColor color, int thickness) 
        {
            double w = System.Convert.ToDouble(rect.Width);
            double h = System.Convert.ToDouble(rect.Height);
            base.Draw(new Rectangle<double>(MapPoint<T>(rect.Center), w/Resolution.X, h/Resolution.Y ), color, thickness);
        }

        /// <summary>
        /// Draw a line segment in the map
        /// </summary>
        /// <typeparam name="T">The type of the line</typeparam>
        /// <param name="line">The line to be draw</param>
        /// <param name="color">The color for the line</param>
        /// <param name="thickness">The thickness of the line</param>
        public override void Draw<T>(LineSegment2D<T> line, TColor color, int thickness) 
        {
            Point2D<double> p1 = MapPoint<T>(line.P1);
            Point2D<double> p2 = MapPoint<T>(line.P2);
            base.Draw(new LineSegment2D<double>(p1, p2), color, thickness);
        }

        ///<summary> Draw a Circle of the specific color and thickness </summary>
        ///<param name="circle"> The circle to be drawn</param>
        ///<param name="color"> The color of the circle </param>
        ///<param name="thickness"> If thickness is less than 1, the circle is filled up </param>
        public override void Draw<T>(Circle<T> circle, TColor color, int thickness) 
        {
            Point2D<double> center = MapPoint<T>(circle.Center);
            double radius = System.Convert.ToDouble(circle.Radius) / Resolution.X;
            base.Draw(new Circle<double>(center, radius), color, thickness);
        }

        ///<summary> Draw a convex polygon of the specific color and thickness </summary>
        ///<param name="polygon"> The convex polygon to be drawn</param>
        ///<param name="color"> The color of the convex polygon </param>
        ///<param name="thickness"> If thickness is less than 1, the triangle is filled up </param>
        public override void Draw<T>(IConvexPolygon<T> polygon, TColor color, int thickness) 
        {
            MCvPoint[] pts = Array.ConvertAll<Point2D<T>, MCvPoint>(
                polygon.Vertices,
                delegate(Point2D<T> p)
                {
                    return MapPoint(p).MCvPoint;
                });
            if (thickness > 0)
                base.DrawPolyline(pts, true, color, thickness);
            else
                base.FillConvexPoly(pts, color);
        }

        /// <summary>
        /// Draw the text using the specific font on the image
        /// </summary>
        /// <param name="message">The text message to be draw</param>
        /// <param name="font">The font used for drawing</param>
        /// <param name="bottomLeft">The location of the bottom left corner of the font</param>
        /// <param name="color">The color of the text</param>
        public override void Draw<T>(String message, ref MCvFont font, Point2D<T> bottomLeft, TColor color) 
        {
            base.Draw(message, ref font, MapPoint<T>(bottomLeft).Convert<int>(), color);
        }

        /// <summary>
        /// Draw the polyline defined by the array of 2D points
        /// </summary>
        /// <param name="pts">the points that defines the poly line</param>
        /// <param name="isClosed">if true, the last line segment is defined by the last point of the array and the first point of the array</param>
        /// <param name="color">the color used for drawing</param>
        /// <param name="thickness">the thinkness of the line</param>
        public override void DrawPolyline<T>(Point2D<T>[] pts, bool isClosed, TColor color, int thickness)
        {
            base.DrawPolyline<double>(
                Array.ConvertAll<Point2D<T>, Point2D<double>>(pts, delegate(Point2D<T> p) { return MapPoint(p); }),
                isClosed,
                color,
                thickness);
        }

        /// <summary>
        /// Compute a new map where each element is obtained from converter
        /// </summary>
        /// <typeparam name="D2">The depth of the new Map</typeparam>
        /// <param name="converter">The converter that use the element from <i>this</i> map and the location of each pixel as input to compute the result</param>
        /// <returns> A new map where each element is obtained from converter</returns>
        public Map<TColor, D2> Convert<D2>(Emgu.Utils.Func<TDepth, double, double, D2> converter)
        {
            double rx = Resolution.X, ry = Resolution.Y, ox = Area.Left, oy = Area.Bottom;

            Emgu.Utils.Func<TDepth, int, int, D2> iconverter =
                delegate(TDepth data, int row, int col)
                {
                    //convert an int position to double position
                    return converter(data, col * rx + ox, row * ry + oy);
                };
            return new Map<TColor,D2>(base.Convert<D2>(iconverter), Area);
        }
    }
}
