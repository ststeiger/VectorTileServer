#if false


namespace SpriteGenerator
{


    public class Sprite2
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }


    // <PackageReference Include="Microsoft.Z3" Version="4.12.2" />
    public static class Sprite2PackingZ3
    {
        public static void Solve(
            System.Collections.Generic.List<Sprite2> Sprite2s, 
            int binWidth
        )
        {
            using Microsoft.Z3.Context ctx = new Microsoft.Z3.Context();

            int n = Sprite2s.Count;
            Microsoft.Z3.IntExpr[] x = new Microsoft.Z3.IntExpr[n];
            Microsoft.Z3.IntExpr[] y = new Microsoft.Z3.IntExpr[n];
            Microsoft.Z3.IntExpr binHeight = ctx.MkIntConst("binHeight");

            Microsoft.Z3.Solver solver = ctx.MkSolver();

            // Bounds for each Sprite2's position
            for (int i = 0; i < n; i++)
            {
                x[i] = ctx.MkIntConst($"x_{i}");
                y[i] = ctx.MkIntConst($"y_{i}");

                // Ensure Sprite2 fits horizontally
                solver.Assert(ctx.MkLe(ctx.MkAdd(x[i], ctx.MkInt(Sprite2s[i].Width)), ctx.MkInt(binWidth)));

                // Ensure Sprite2 fits vertically
                solver.Assert(ctx.MkLe(ctx.MkAdd(y[i], ctx.MkInt(Sprite2s[i].Height)), binHeight));

                // Non-negative positions
                solver.Assert(ctx.MkGe(x[i], ctx.MkInt(0)));
                solver.Assert(ctx.MkGe(y[i], ctx.MkInt(0)));
            }

            // No-overlap constraint
            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    Microsoft.Z3.BoolExpr noOverlap = ctx.MkOr(
                        ctx.MkLe(ctx.MkAdd(x[i], ctx.MkInt(Sprite2s[i].Width)), x[j]),
                        ctx.MkLe(ctx.MkAdd(x[j], ctx.MkInt(Sprite2s[j].Width)), x[i]),
                        ctx.MkLe(ctx.MkAdd(y[i], ctx.MkInt(Sprite2s[i].Height)), y[j]),
                        ctx.MkLe(ctx.MkAdd(y[j], ctx.MkInt(Sprite2s[j].Height)), y[i])
                    );
                    solver.Assert(noOverlap);
                }
            }

            // Try minimal height with binary search
            int totalArea = 0;
            int maxHeight = 0;
            foreach (var s in Sprite2s)
            {
                totalArea += s.Width * s.Height;
                maxHeight += s.Height;
            }

            int low = 0, high = maxHeight;
            Microsoft.Z3.Model? bestModel = null;
            while (low < high)
            {
                int mid = (low + high) / 2;

                Microsoft.Z3.Solver testSolver = ctx.MkSolver();
                foreach (var a in solver.Assertions)
                    testSolver.Assert(a);

                testSolver.Assert(ctx.MkLe(binHeight, ctx.MkInt(mid)));

                if (testSolver.Check() == Microsoft.Z3.Status.SATISFIABLE)
                {
                    bestModel = testSolver.Model;
                    high = mid;
                }
                else
                {
                    low = mid + 1;
                }
            }

            if (bestModel != null)
            {
                System.Console.WriteLine($"Minimal packing height: {high}");
                for (int i = 0; i < n; i++)
                {
                    int xi = ((Microsoft.Z3.IntNum)bestModel.Evaluate(x[i])).Int;
                    int yi = ((Microsoft.Z3.IntNum)bestModel.Evaluate(y[i])).Int;
                    System.Console.WriteLine($"Sprite2 {i}: x={xi}, y={yi}, w={Sprite2s[i].Width}, h={Sprite2s[i].Height}");
                }
            }
            else
            {
                System.Console.WriteLine("No solution found.");
            }
        } // End Sub Solve 


    } // End Class Sprite2PackingZ3 


} // End Namespace 


#endif
