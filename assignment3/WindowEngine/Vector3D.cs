using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace WindowEngine
{
    class Vector3D
    {
        Vector3 a = new Vector3(1, 4, 5);
        Vector3 b = new Vector3(8, 9, 11);

        public void Calculate()
        {
            // Vector addition
            Vector3 add = a + b;
            
            //Dot production
            float dot = Vector3.Dot(a, b);

            //Cross product 
            Vector3 cross = Vector3.Cross(a, b);
            
            //Normalize
            Vector3 normalized = a.Normalized();

            // Output results
            Console.WriteLine($"Add: {add}");
            Console.WriteLine($"Dot: {dot}");
            Console.WriteLine($"Cross: {cross}");
            Console.WriteLine($"Normalized a: {normalized}");
        }
    }
}
