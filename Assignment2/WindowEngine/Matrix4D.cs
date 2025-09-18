using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace WindowEngine
{
    class Matrix4D
    {
        Matrix4 scale = Matrix4.CreateScale(2.0f);
        Matrix4 rotation = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(90));
        Matrix4 translation = Matrix4.CreateTranslation(1, 2, 0);
        public void CalculateMatrix()
        {
            //Combine the transformations
            Matrix4 transform = scale * rotation * translation;

            //Point to transform 
            Vector4 point = new Vector4(1, 0, 0, 1);
            
            //Apply the transformation
            Vector4 result = Vector4.TransformRow(point, transform);
            
            // Output result
            Console.WriteLine($"Matrix Result: {result}");
        }
    }
}
