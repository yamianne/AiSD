using System;
using System.Collections.Generic;
using System.Linq;
using static ASD.Geometry;

namespace ASD
{
    public static class SutherlandHodgman
    {
        /// <summary>
        /// Oblicza pole wielokata przy pomocy formuly Gaussa
        /// </summary>
        /// <param name="polygon">Kolejne wierzcholki wielokata</param>
        /// <returns>Pole wielokata</returns>
        public static double PolygonArea(this Point[] polygon)
        {
            double area = 0;
            for (int i = 1; i < polygon.Length - 1; i++)
                area += Point.CrossProduct(polygon[i] - polygon[0], polygon[i + 1] - polygon[0]);
            area = 0.5 * Math.Abs(area);
            return area;
        }

        /// <summary>
        /// Sprawdza, czy dwa punkty leza po tej samej stronie prostej wyznaczonej przez odcinek s
        /// </summary>
        /// <param name="p1">Pierwszy punkt</param>
        /// <param name="p2">Drugi punkt</param>
        /// <param name="s">Odcinek wyznaczajacy prosta</param>
        /// <returns>
        /// True, jesli punkty leza po tej samej stronie prostej wyznaczonej przez odcinek 
        /// (lub co najmniej jeden z punktow lezy na prostej). W przeciwnym przypadku zwraca false.
        /// </returns>
        public static bool IsSameSide(Point p1, Point p2, Segment s)
        {
            if (s.Direction(p1) == s.Direction(p2) || s.Direction(p1) == 0 || s.Direction(p2) == 0)
                return true;
            return false;
        }

        /// <summary>Oblicza czesc wspolna dwoch wielokatow przy pomocy algorytmu Sutherlanda–Hodgmana</summary>
        /// <param name="subjectPolygon">Wielokat obcinany (wklesly lub wypukly)</param>
        /// <param name="clipPolygon">Wielokat obcinajacy (musi byc wypukly i zakladamy, ze taki jest)</param>
        /// <returns>Czesc wspolna wielokatow</returns>
        /// <remarks>
        /// - mozna zalozyc, ze 3 kolejne punkty w kazdym z wejsciowych wielokatow nie sa wspolliniowe
        /// - wynikiem dzialania funkcji moze byc tak naprawde wiele wielokatow (sytuacja taka moze wystapic,
        ///   jesli wielokat obcinany jest wklesly)
        /// - jesli wielokat obcinany i obcinajacy zawieraja wierzcholki o tych samych wspolrzednych,
        ///   w wynikowym wielokacie moge one byc zduplikowane
        /// - wierzcholki wielokata obcinanego, przez ktore przechodza krawedzie wielokata obcinajacego
        ///   zostaja zduplikowane w wielokacie wyjsciowym
        /// </remarks>
        public static Point[] GetIntersectedPolygon(Point[] subjectPolygon, Point[] clipPolygon)
        {
            List<Point> PolygonPoints = new List<Point>();
            foreach (Point p in subjectPolygon)
                PolygonPoints.Add(p);
            List<Segment> clipPolygonEdges = new List<Segment>();
            for (int i = 0; i < clipPolygon.Length; i++)
                clipPolygonEdges.Add(new Segment(clipPolygon[i], clipPolygon[(i + 1) % clipPolygon.Length]));
            for (int i = 0; i < clipPolygonEdges.Count; i++)
            {
                Segment seg = clipPolygonEdges[i];
                List<Point> tmp = new List<Point>(PolygonPoints);
                PolygonPoints.Clear();
                Point last = tmp[tmp.Count - 1];
                foreach (Point p in tmp)
                {
                    Point magicPoint = clipPolygonEdges[(i+1) % clipPolygonEdges.Count].pe;
                    if (IsSameSide(p, magicPoint, seg))
                    {
                        if (!IsSameSide(last, magicPoint, seg))
                            PolygonPoints.Add(GetIntersectionPoint(new Segment(last, p), seg));
                        PolygonPoints.Add(p);
                    }
                    else if (IsSameSide(last, magicPoint, seg))
                        PolygonPoints.Add(GetIntersectionPoint(new Segment(last, p), seg));
                    last = p;

                }
            }

            for (int i = 0; i < PolygonPoints.Count - 1; i++)
            {
                if (PolygonPoints[i] == PolygonPoints[(i + 1) % PolygonPoints.Count])
                {
                    PolygonPoints.RemoveAt(i);
                    i--;
                }
            }

                //var cos = PolygonPoints.Distinct();

            return PolygonPoints.ToArray();
        }

        /// <summary>
        /// Zwraca punkt przeciecia dwoch prostych wyznaczonych przez odcinki
        /// </summary>
        /// <param name="seg1">Odcinek pierwszy</param>
        /// <param name="seg2">Odcinek drugi</param>
        /// <returns>Punkt przeciecia prostych wyznaczonych przez odcinki</returns>
        public static Point GetIntersectionPoint(Segment seg1, Segment seg2)
        {
            Point direction1 = new Point(seg1.pe.x - seg1.ps.x, seg1.pe.y - seg1.ps.y);
            Point direction2 = new Point(seg2.pe.x - seg2.ps.x, seg2.pe.y - seg2.ps.y);
            double dotPerp = (direction1.x * direction2.y) - (direction1.y * direction2.x);

            Point c = new Point(seg2.ps.x - seg1.ps.x, seg2.ps.y - seg1.ps.y);
            double t = (c.x * direction2.y - c.y * direction2.x) / dotPerp;

            return new Point(seg1.ps.x + (t * direction1.x), seg1.ps.y + (t * direction1.y));
        }
    }
}
