using System;
using System.Collections.Generic;
using System.Linq;
using static ASD.Geometry;

namespace ASD
{
    class IsSameSideTestCase : TestCase
    {
        private bool result;
        private bool expectedResult;
        private Point p1;
        private Point p2;
        private Segment s;

        public IsSameSideTestCase(double timeLimit, Point p1, Point p2, Segment s, bool expectedResult) : base(timeLimit, null)
        {
            this.expectedResult = expectedResult;
            this.p1 = p1;
            this.p2 = p2;
            this.s = s;
        }

        public override void PerformTestCase()
        {
            result = SutherlandHodgman.IsSameSide(p1, p2, s);
        }

        public override void VerifyTestCase(out Result resultCode, out string message)
        {
            if (result != expectedResult)
            {
                resultCode = Result.BadResult;
                message = $"incorrect result: {result} (expected: {expectedResult})";
                return;
            }
            resultCode = Result.Success;
            message = "OK";
        }
    }

    class PolygonAreaTestCase : TestCase
    {
        private double result;
        private double expectedResult;
        private Point[] polygon;
        private static double EPS = 0.000001;

        public PolygonAreaTestCase(double timeLimit, Point[] polygon, double expectedResult) : base(timeLimit, null)
        {
            this.expectedResult = expectedResult;
            this.polygon = polygon;
        }

        public override void PerformTestCase()
        {
            result = SutherlandHodgman.PolygonArea(polygon);
        }

        public override void VerifyTestCase(out Result resultCode, out string message)
        {
            if (Math.Abs(result - expectedResult) > EPS)
            {
                resultCode = Result.BadResult;
                message = $"incorrect result: {result} (expected: {expectedResult})";
                return;
            }
            resultCode = Result.Success;
            message = "OK";
        }
    }

    class SutherlandHodgmanTestCase : TestCase
    {
        private Point[] intersectedPolygon;
        private Point[] expectedIntersectedPolygon;
        private Point[] subjectPolygon;
        private Point[] clipPolygon;

        public SutherlandHodgmanTestCase(double timeLimit, Point[] subjectPolygon, Point[] clipPolygon, Point[] expectedIntersectedPolygon) : base(timeLimit, null)
        {
            this.subjectPolygon = subjectPolygon;
            this.clipPolygon = clipPolygon;
            this.expectedIntersectedPolygon = expectedIntersectedPolygon;
        }

        public override void PerformTestCase()
        {
            intersectedPolygon = SutherlandHodgman.GetIntersectedPolygon(subjectPolygon, clipPolygon);
        }

        public override void VerifyTestCase(out Result resultCode, out string message)
        {
            if (intersectedPolygon == null)
            {
                resultCode = Result.BadResult;
                message = $"incorrect result: null";
                return;
            }
            if (intersectedPolygon.Length != expectedIntersectedPolygon.Length)
            {
                resultCode = Result.BadResult;
                message = $"incorrect result: " + PolygonToString(intersectedPolygon) + " (expected: " + PolygonToString(expectedIntersectedPolygon) + ")";
                return;
            }

            if(intersectedPolygon.Length == 0)  // wiemy ze expected tez jest 0
            {
                resultCode = Result.Success;
                message = "OK";
                return;
            }

            int startIndex;// = Array.IndexOf(intersectedPolygon, expectedIntersectedPolygon[0]);
            for ( startIndex=0 ; startIndex<intersectedPolygon.Length ; ++startIndex )
                if ( Point.EpsilonEquals(intersectedPolygon[startIndex],expectedIntersectedPolygon[0]) )
                    break;
//            if(startIndex == -1)
            if( startIndex==intersectedPolygon.Length )
            {
                resultCode = Result.BadResult;
                message = $"incorrect result: " + PolygonToString(intersectedPolygon) + " (expected: " + PolygonToString(expectedIntersectedPolygon) + ")";
                return;
            }

            bool isOrderCorrect = true;
            for (int i = 0, index = startIndex; i < intersectedPolygon.Length; i++, index++)
            {
                if( !Point.EpsilonEquals(intersectedPolygon[index%intersectedPolygon.Length],expectedIntersectedPolygon[i]) )
                {
                    isOrderCorrect = false;
                    break;
                }
            }
            if(isOrderCorrect)
            {
                resultCode = Result.Success;
                message = "OK";
                return;
            }

            //Sprawdzmy w przeciwna strone
            isOrderCorrect = true;
            for (int i = 0, index = startIndex; i < intersectedPolygon.Length; i++, index--)
            {
                if ( !Point.EpsilonEquals(intersectedPolygon[(index+intersectedPolygon.Length)%intersectedPolygon.Length],expectedIntersectedPolygon[i]) )
                {
                    isOrderCorrect = false;
                    break;
                }
            }
            if (isOrderCorrect)
            {
                resultCode = Result.Success;
                message = "OK";
                return;
            }
            resultCode = Result.BadResult;
            message = $"incorrect result: " + PolygonToString(intersectedPolygon) + " (expected: " + PolygonToString(expectedIntersectedPolygon) + ")";
        }

        static String PolygonToString(Point[] polygon)
        {
            String result = "";
            for (int i = 0; i < polygon.Length; ++i)
                result += polygon[i] + " ";
            return result;
        }
    }

    class BitmapDrawer
    {
        Point[] subjectPolygon;
        Point[] clipPolygon;
        Point[] resultPolygon;
        int testCount;

        const int bitmapSize = 700;
        const int bitsPerUnit = 30;
        const int range = 10;
        static System.Drawing.Color subjectPolygonColor = System.Drawing.Color.Blue;
        static System.Drawing.Color clipPolygonColor = System.Drawing.Color.Red;
        static System.Drawing.Color resultPolygonColor = System.Drawing.Color.Orange;
        static System.Drawing.Color commonColor = System.Drawing.Color.Green;   //gdy wynikowy pokrywa się z obcinanym/obcinajacym


        public BitmapDrawer(Point[] subjectPolygon, Point[] clipPolygon, Point[] resultPolygon, int testCount)
        {
            this.subjectPolygon = subjectPolygon;
            this.clipPolygon = clipPolygon;
            this.resultPolygon = resultPolygon;
            this.testCount = testCount;
            drawAndSaveImage();
        }

        private void drawAndSaveImage()
        {
            System.Drawing.Bitmap image = new System.Drawing.Bitmap(bitmapSize, bitmapSize);
            drawWhiteBitmap(image);
            drawAxes(image);
            drawGraduation(image);

            drawPolygon(subjectPolygon, subjectPolygonColor, image, false);
            drawPolygon(clipPolygon, clipPolygonColor, image, false);
            drawPolygon(resultPolygon, resultPolygonColor, image, true);

            image.Save("../../Testy/Test" + testCount.ToString() + ".bmp");
        }

        private void drawWhiteBitmap(System.Drawing.Bitmap image)
        {
            for (int i = 0; i < bitmapSize; i++)
                for (int j = 0; j < bitmapSize; j++)
                    image.SetPixel(i, j, System.Drawing.Color.White);
        }

        private void drawAxes(System.Drawing.Bitmap image)
        {
            for (int i = 0; i < bitmapSize; i++)
            {
                image.SetPixel(i, bitmapSize / 2, System.Drawing.Color.Black);
                image.SetPixel(bitmapSize / 2, i, System.Drawing.Color.Black);
            }
        }

        private void drawGraduation(System.Drawing.Bitmap image)
        {
            for (int i = 0; i < 2 * range + 1; i++)
            {
                for (int j = -4; j < 5; j++)
                {
                    image.SetPixel(bitmapSize / 2 - range * bitsPerUnit + i * bitsPerUnit, bitmapSize / 2 + j, System.Drawing.Color.Black);
                    image.SetPixel(bitmapSize / 2 + j, bitmapSize / 2 - range * bitsPerUnit + i * bitsPerUnit, System.Drawing.Color.Black);
                }
            }
        }

        private int coordinateYToBit(double coordinate)
        {
            return (int)(bitmapSize / 2 - coordinate * bitsPerUnit);
        }

        private int coordinateXToBit(double coordinate)
        {
            return (int)(bitmapSize / 2 + coordinate * bitsPerUnit);
        }

        private void drawPolygon(Point[] polygon, System.Drawing.Color color, System.Drawing.Bitmap image, bool resultPolygon)
        {
            if (polygon != null)
                //drawVertices(polygon, color, image, resultPolygon);
                drawEdges(polygon, color, image, resultPolygon);
        }

        private void drawVertices(Point[] polygon, System.Drawing.Color color, System.Drawing.Bitmap image, bool resultPolygon)
        {
            foreach (Point p in polygon)
            {
                for (int i = -2; i < 3; i++)
                    for (int j = -2; j < 3; j++)
                        setProperColor(image, coordinateXToBit(p.x) + i, coordinateYToBit(p.y) + j, color, resultPolygon);
            }
        }
        private void drawEdges(Point[] polygon, System.Drawing.Color color, System.Drawing.Bitmap image, bool resultPolygon)
        {
            for (int i = 0; i < polygon.Length; i++)
            {
                Point p1 = polygon[i % polygon.Length];
                Point p2 = polygon[(i + 1) % polygon.Length];

                if (p1.x == p2.x)
                {
                    for (int j = Math.Min(coordinateYToBit(p1.y), coordinateYToBit(p2.y)); j <= Math.Max(coordinateYToBit(p1.y), coordinateYToBit(p2.y)); j++)
                        setProperColor(image, coordinateXToBit(p1.x), j, color, resultPolygon);
                }
                else if (p1.y == p2.y)
                {
                    for (int j = Math.Min(coordinateXToBit(p1.x), coordinateXToBit(p2.x)); j <= Math.Max(coordinateXToBit(p1.x), coordinateXToBit(p2.x)); j++)
                        setProperColor(image, j, coordinateYToBit(p1.y), color, resultPolygon);
                }
                else
                {
                    if (p1.x > p2.x)
                    {
                        Point temp = p1;
                        p1 = p2;
                        p2 = temp;
                    }
                    double a = (p2.y - p1.y) / (p2.x - p1.x);
                    for (int j = coordinateXToBit(p1.x); j <= coordinateXToBit(p2.x); j++)
                        setProperColor(image, j, (int)(coordinateYToBit(p1.y) - a * (j - coordinateXToBit(p1.x))), color, resultPolygon);
                }
            }
        }

        private void setProperColor(System.Drawing.Bitmap image, int x, int y, System.Drawing.Color color, bool resultPolygon)
        {
            if (!resultPolygon)
                image.SetPixel(x, y, color);
            else
            {
                System.Drawing.Color properColor;
                System.Drawing.Color sampledColor = image.GetPixel(x, y);
                if (sampledColor.ToArgb() == subjectPolygonColor.ToArgb() || sampledColor.ToArgb() == clipPolygonColor.ToArgb() ||
                    sampledColor.ToArgb() == resultPolygonColor.ToArgb())
                    properColor = commonColor;
                else
                    properColor = resultPolygonColor;
                image.SetPixel(x, y, properColor);
            }
        }
    }

    class Program
    {
        static void Main()
        {
            bool drawBitmaps = false;

            TestSet isSameSideTests = new TestSet();
            isSameSideTests.TestCases.Add(new IsSameSideTestCase(5, new Point(0, 1), new Point(2, 1), new Segment(new Point(1, 1), new Point(4, 4)), false));
            isSameSideTests.TestCases.Add(new IsSameSideTestCase(5, new Point(2, 1), new Point(0, 1), new Segment(new Point(1, 1), new Point(4, 4)), false));
            isSameSideTests.TestCases.Add(new IsSameSideTestCase(5, new Point(1, 3), new Point(2, 6), new Segment(new Point(1, 1), new Point(4, 4)), true));
            isSameSideTests.TestCases.Add(new IsSameSideTestCase(5, new Point(2, 6), new Point(1, 3), new Segment(new Point(1, 1), new Point(4, 4)), true));
            isSameSideTests.TestCases.Add(new IsSameSideTestCase(5, new Point(2, 2), new Point(3, 3), new Segment(new Point(1, 1), new Point(4, 4)), true));
            isSameSideTests.TestCases.Add(new IsSameSideTestCase(5, new Point(2, 2), new Point(-1, -1), new Segment(new Point(1, 1), new Point(4, 4)), true));
            isSameSideTests.TestCases.Add(new IsSameSideTestCase(5, new Point(2, 2), new Point(5, 3), new Segment(new Point(1, 1), new Point(4, 4)), true));
            isSameSideTests.TestCases.Add(new IsSameSideTestCase(5, new Point(2, 2), new Point(1, 3), new Segment(new Point(1, 1), new Point(4, 4)), true));
            isSameSideTests.TestCases.Add(new IsSameSideTestCase(5, new Point(2, 2), new Point(1, 3), new Segment(new Point(-5, -2), new Point(-1, -4)), true));
            isSameSideTests.TestCases.Add(new IsSameSideTestCase(5, new Point(0, 0), new Point(1, 1), new Segment(new Point(-5, -2), new Point(-1, -4)), true));
            isSameSideTests.TestCases.Add(new IsSameSideTestCase(5, new Point(0, 0), new Point(-3, -4), new Segment(new Point(-5, -2), new Point(-1, -4)), false));
            isSameSideTests.TestCases.Add(new IsSameSideTestCase(5, new Point(0, 0), new Point(-10, 0), new Segment(new Point(-5, -2), new Point(-1, -4)), false));
            isSameSideTests.TestCases.Add(new IsSameSideTestCase(5, new Point(2, 3), new Point(-3, 4), new Segment(new Point(-5, 5), new Point(0, 0)), true));
            isSameSideTests.TestCases.Add(new IsSameSideTestCase(5, new Point(-4, 1), new Point(-1, 4), new Segment(new Point(-5, 5), new Point(0, 0)), false));

            TestSet polygonAreaTests = new TestSet();
            polygonAreaTests.TestCases.Add(new PolygonAreaTestCase(5, new Point[] { new Point(0, 0) }, 0));
            polygonAreaTests.TestCases.Add(new PolygonAreaTestCase(5, new Point[] { new Point(1, 1), new Point(2, 2) }, 0));
            polygonAreaTests.TestCases.Add(new PolygonAreaTestCase(5, new Point[] { new Point(1, 1), new Point(2, 2), new Point(2, 0) }, 1));
            polygonAreaTests.TestCases.Add(new PolygonAreaTestCase(5, new Point[] { new Point(-1, -1), new Point(2, -1), new Point(2, 3),
                new Point(-1, 3) }, 12));
            polygonAreaTests.TestCases.Add(new PolygonAreaTestCase(5, new Point[] { new Point(3, 4), new Point(5, 11), new Point(12, 8),
                new Point(9, 5), new Point(5, 6)}, 30));
            polygonAreaTests.TestCases.Add(new PolygonAreaTestCase(5, new Point[] { new Point(-5, 0), new Point(-3, 0), new Point(-2, -1),
                new Point(0, -1), new Point(0, 3), new Point(-2, 1), new Point(-3, 2)}, 10));
            polygonAreaTests.TestCases.Add(new PolygonAreaTestCase(5, new Point[] { new Point(-3, -2), new Point(-4, -4), new Point(-2, -3), new Point(0, -3),
                new Point(0, -5), new Point(5, 0), new Point(0, 0), new Point(0, 2), new Point(-1, 2), new Point(-1, 1), new Point(-6, 1)}, 31));
            polygonAreaTests.TestCases.Add(new PolygonAreaTestCase(5, GenerateRandomPolygon(12, 111), 120.5));
            polygonAreaTests.TestCases.Add(new PolygonAreaTestCase(5, GenerateRandomPolygon(16, 222), 172));
            polygonAreaTests.TestCases.Add(new PolygonAreaTestCase(5, GenerateRandomPolygon(20, 111), 139));

            TestSet sutherlandHodgmanTests = new TestSet();
            Point[] sp1 = new Point[] { new Point(2, 1), new Point(4, 1), new Point(4, 3), new Point(2, 3) };
            Point[] cp1 = new Point[] { new Point(3, 2), new Point(5, 2), new Point(5, 4), new Point(3, 4) };
            Point[] sh1 = new Point[] { new Point(3, 2), new Point(4, 2), new Point(4, 3), new Point(3, 3) };
            sutherlandHodgmanTests.TestCases.Add(new SutherlandHodgmanTestCase(5, sp1, cp1, sh1));

            Point[] sp2 = new Point[] { new Point(2, 1), new Point(4, 1), new Point(4, 3), new Point(2, 3) };
            Point[] cp2 = new Point[] { new Point(5, 2), new Point(7, 2), new Point(7, 4), new Point(5, 4) };
            Point[] sh2 = new Point[] { };
            sutherlandHodgmanTests.TestCases.Add(new SutherlandHodgmanTestCase(5, sp2, cp2, sh2));

            Point[] sp3 = new Point[] { new Point(-1, 1), new Point(0, 1), new Point(0, 2), new Point(1, 2), new Point(1, 1),
                new Point(2, 1), new Point(2, 3), new Point(-1, 3)};
            Point[] cp3 = new Point[] { new Point(-1, 0), new Point(2, 0), new Point(2, 2), new Point(-1, 2) };
            Point[] sh3 = new Point[] { new Point(-1, 2), new Point(-1, 1), new Point(0, 1), new Point(0, 2), new Point(1, 2),
                new Point(1, 1), new Point(2, 1), new Point(2, 2)};
            sutherlandHodgmanTests.TestCases.Add(new SutherlandHodgmanTestCase(5, sp3, cp3, sh3));

            Point[] sp4 = new Point[] { new Point(2, 1), new Point(4, 1), new Point(4, 3), new Point(2, 3) };
            Point[] cp4 = new Point[] { new Point(1, 0), new Point(5, 0), new Point(5, 4), new Point(1, 4) };
            Point[] sh4a = new Point[] { new Point(2, 1), new Point(4, 1), new Point(4, 3), new Point(2, 3) };
            sutherlandHodgmanTests.TestCases.Add(new SutherlandHodgmanTestCase(5, sp4, cp4, sh4a));

            Point[] sh4b = new Point[] { new Point(2, 3), new Point(2, 1), new Point(4, 1), new Point(4, 3) };
            sutherlandHodgmanTests.TestCases.Add(new SutherlandHodgmanTestCase(5, cp4, sp4, sh4b));

            Point[] sp5 = new Point[] { new Point(2, 2), new Point(3, 2), new Point(3, 4), new Point(4, 4),
                new Point(4, 2), new Point(5, 2), new Point(5, 5), new Point(2, 5) };
            Point[] cp5a = new Point[] { new Point(1, 3), new Point(6, 3), new Point(6, 6), new Point(1, 6) };
            Point[] sh5a = new Point[] { new Point(2, 3), new Point(3, 3), new Point(3, 4), new Point(4, 4),
                new Point(4, 3), new Point(5, 3), new Point(5, 5), new Point(2, 5) };
            sutherlandHodgmanTests.TestCases.Add(new SutherlandHodgmanTestCase(5, sp5, cp5a, sh5a));

            Point[] cp5b = new Point[] { new Point(1, 1), new Point(6, 1), new Point(6, 3), new Point(1, 3) };
            Point[] sh5b = new Point[] { new Point(2, 3), new Point(2, 2), new Point(3, 2), new Point(3, 3),
                new Point(4, 3), new Point(4, 2), new Point(5, 2), new Point(5, 3) };
            sutherlandHodgmanTests.TestCases.Add(new SutherlandHodgmanTestCase(5, sp5, cp5b, sh5b));

            Point[] sp6 = GenerateRandomPolygon(16, 222);
            Point[] cp6 = new Point[] { new Point(-9, -3), new Point(8.5, -3), new Point(8.5, 4), new Point(-9, 4) };
            Point[] sh6 = new Point[] { new Point(6.8, -3), new Point(8, 0), new Point(8.5, 0.5), new Point(8.5, 2),
                new Point(7.5, 4), new Point(-6, 4), new Point(-6, 2), new Point(-5, 0), new Point(-8, -2), new Point(-7.25, -3)};
            sutherlandHodgmanTests.TestCases.Add(new SutherlandHodgmanTestCase(5, sp6, cp6, sh6));

            Point[] sp7 = GenerateRandomPolygon(20, 111);
            Point[] cp7a = new Point[] { new Point(-8, -5), new Point(-6, -5), new Point(-6, 5), new Point(-8, 5) };
            Point[] sh7a = new Point[] { new Point(-6, 4), new Point(-7, 4), new Point(-6, 3), new Point(-6, -0.75),
                new Point(-8, -2.25), new Point(-8, -3.2), new Point(-6, -3.6) };
            sutherlandHodgmanTests.TestCases.Add(new SutherlandHodgmanTestCase(5, sp7, cp7a, sh7a));

            Point[] cp7b = new Point[] { new Point(4, -3), new Point(-4, -7), new Point(0, -7) };
            Point[] sh7b = new Point[] { new Point(4, -3), new Point(-2.8, -6.4), new Point(-2.5, -7), new Point(-1, -7),
                new Point(0, -6), new Point(0.5, -6.5) };
            sutherlandHodgmanTests.TestCases.Add(new SutherlandHodgmanTestCase(5, sp7, cp7b, sh7b));


            TestSet sutherlandHodgmanTestsWithDuplicates = new TestSet();
            Point[] dsp1 = new Point[] { new Point(2, 1), new Point(4, 1), new Point(4, 3), new Point(2, 3) };
            Point[] dcp1a = new Point[] { new Point(4, 1), new Point(6, 1), new Point(6, 3), new Point(4, 3) };
            Point[] dsh1a = new Point[] { new Point(4, 1), new Point(4, 3) };
            sutherlandHodgmanTestsWithDuplicates.TestCases.Add(new SutherlandHodgmanTestCase(5, dsp1, dcp1a, dsh1a));

            Point[] dcp1b = new Point[] { new Point(5, 4), new Point(3, 2), new Point(5, 0), new Point(7, 2) };
            Point[] dsh1b = new Point[] { new Point(4, 3), new Point(3, 2), new Point(4, 1) };
            sutherlandHodgmanTestsWithDuplicates.TestCases.Add(new SutherlandHodgmanTestCase(5, dsp1, dcp1b, dsh1b));

            Point[] dsp2 = new Point[] { new Point(0, 0), new Point(1, 0), new Point(1, 1), new Point(2, 1),
                new Point(2, 0), new Point(3, 0), new Point(3, 4), new Point(0, 4)};
            Point[] dch2 = new Point[] { new Point(0, 0), new Point(1, 0), new Point(1, 5), new Point(0, 5) };
            Point[] dsh2 = new Point[] { new Point(0, 0), new Point(1, 0), new Point(1, 1), new Point(1, 4), new Point(0, 4) };
            sutherlandHodgmanTestsWithDuplicates.TestCases.Add(new SutherlandHodgmanTestCase(5, dsp2, dch2, dsh2));

            Point[] dsp3 = GenerateRandomPolygon(14, 12345);
            Point[] dch3 = new Point[] { new Point(5, 7), new Point(-7, 5), new Point(-7, 0), new Point(5, 0) };
            Point[] dsh3 = new Point[] { new Point(5, 0), new Point(5, 6), new Point(4, 6), new Point(2.75, 6.625),
                new Point(-5.8, 5.2), new Point(-5, 2), new Point(-7, 2), new Point(-7, 1), new Point (-6, 0)};
            sutherlandHodgmanTestsWithDuplicates.TestCases.Add(new SutherlandHodgmanTestCase(5, dsp3, dch3, dsh3));

            Point[] dsp4 = GenerateRandomPolygon(16, 222);
            Point[] dcp4 = new Point[] { new Point(-9, -2), new Point(8, -2), new Point(8, 4), new Point(-9, 4) };
            Point[] dsh4 = new Point[] { new Point(7.2, -2), new Point(8, 0), new Point(8, 3), new Point(7.5, 4),
                new Point(-6, 4), new Point(-6, 2), new Point(-5, 0), new Point(-8, -2) };
            sutherlandHodgmanTestsWithDuplicates.TestCases.Add(new SutherlandHodgmanTestCase(5, dsp4, dcp4, dsh4));

            Console.WriteLine("IsSameSide tests");
            isSameSideTests.PreformTests(verbose: true, checkTimeLimit: false);

            Console.WriteLine("\nPolygon area tests");
            polygonAreaTests.PreformTests(verbose: true, checkTimeLimit: false);

            Console.WriteLine("\nSutherland-Hodgman tests");
            sutherlandHodgmanTests.PreformTests(verbose: true, checkTimeLimit: false);

            Console.WriteLine("\nSutherland-Hodgman tests with duplicates");
            sutherlandHodgmanTestsWithDuplicates.PreformTests(verbose: true, checkTimeLimit: false);

            if (drawBitmaps)
            {
                System.IO.Directory.CreateDirectory("../../Testy");
                BitmapDrawer bitmapDrawer = new BitmapDrawer(sp1, cp1, SutherlandHodgman.GetIntersectedPolygon(sp1, cp1), 1);
                bitmapDrawer = new BitmapDrawer(sp2, cp2, SutherlandHodgman.GetIntersectedPolygon(sp2, cp2), 2);
                bitmapDrawer = new BitmapDrawer(sp3, cp3, SutherlandHodgman.GetIntersectedPolygon(sp3, cp3), 3);
                bitmapDrawer = new BitmapDrawer(sp4, cp4, SutherlandHodgman.GetIntersectedPolygon(sp4, cp4), 4);
                bitmapDrawer = new BitmapDrawer(cp4, sp4, SutherlandHodgman.GetIntersectedPolygon(cp4, sp4), 5);
                bitmapDrawer = new BitmapDrawer(sp5, cp5a, SutherlandHodgman.GetIntersectedPolygon(sp5, cp5a), 6);
                bitmapDrawer = new BitmapDrawer(sp5, cp5b, SutherlandHodgman.GetIntersectedPolygon(sp5, cp5b), 7);
                bitmapDrawer = new BitmapDrawer(sp6, cp6, SutherlandHodgman.GetIntersectedPolygon(sp6, cp6), 8);
                bitmapDrawer = new BitmapDrawer(sp7, cp7a, SutherlandHodgman.GetIntersectedPolygon(sp7, cp7a), 9);
                bitmapDrawer = new BitmapDrawer(sp7, cp7b, SutherlandHodgman.GetIntersectedPolygon(sp7, cp7b), 10);
                bitmapDrawer = new BitmapDrawer(dsp1, dcp1a, SutherlandHodgman.GetIntersectedPolygon(dsp1, dcp1a), 11);
                bitmapDrawer = new BitmapDrawer(dsp1, dcp1b, SutherlandHodgman.GetIntersectedPolygon(dsp1, dcp1b), 12);
                bitmapDrawer = new BitmapDrawer(dsp2, dch2, SutherlandHodgman.GetIntersectedPolygon(dsp2, dch2), 13);
                bitmapDrawer = new BitmapDrawer(dsp3, dch3, SutherlandHodgman.GetIntersectedPolygon(dsp3, dch3), 14);
                bitmapDrawer = new BitmapDrawer(dsp4, dcp4, SutherlandHodgman.GetIntersectedPolygon(dsp4, dcp4), 15);
            }
        }

        public static Point[] GenerateRandomPolygon(int num_vertices, int seed)
        {
            Random rand = new Random(seed);

            // Wylosuj "promien" dla kazdego punktu.
            double[] radii = new double[num_vertices];
            for (int i = 0; i < num_vertices; i++)
                radii[i] = rand.NextDouble() / 2 + 0.5;

            // Wylosuj wagi katow (ich relatywne wielkosci)
            double[] angle_weights = new double[num_vertices];
            double total_weight = 0;
            for (int i = 0; i < num_vertices; i++)
            {
                angle_weights[i] = rand.NextDouble() * 9 + 1;
                total_weight += angle_weights[i];
            }

            // Zamien wagi katow na ich rzeczywiste miary
            double[] angles = new double[num_vertices];
            double to_radians = 2 * Math.PI / total_weight;
            for (int i = 0; i < num_vertices; i++)
                angles[i] = angle_weights[i] * to_radians;

            // Oblicz miary katow
            List<Point> points = new List<Point>();
            //"Promien" bazowy
            float rx = 10;
            float ry = 10;
            //Srodek
            float cx = 0;
            float cy = 0;
            double theta = 0;
            for (int i = 0; i < num_vertices; i++)
            {
                Point newPoint = new Point(cx + (int)(rx * radii[i] * Math.Cos(theta)), cy + (int)(ry * radii[i] * Math.Sin(theta)));
                if (i == 0 || (i > 0 && newPoint != points[points.Count - 1]))
                    points.Add(newPoint);
                theta += angles[i];
            }

            return points.ToArray();
        }
    }
}
