using UnityEngine;

namespace FOW
{
    class TextureScaler
    {
        private const float blur = 0.5f;

        private Color[] scaled;
        private int originalSize;
        private int scaledSize;

        private delegate void ScaleOperation(int index, Color[] quad, Color revealed, Color blurTime);
        private ScaleOperation[] operations;

        public TextureScaler(int originalSize, int scaledSize)
        {
            this.originalSize = originalSize;
            this.scaledSize = scaledSize;
            scaled = new Color[scaledSize * scaledSize];
            operations = new ScaleOperation[]
            {
                Scale0, Scale1, Scale2, Scale3,
                Scale4, Scale5, Scale6, Scale7,
                Scale8, Scale9, Scale10, Scale11,
                Scale12, Scale13, Scale14, Scale15
            };
        }

        public Color[] Scale(Color[] original, Color time)
        {
            Color[] quad = new Color[4];
            Color revealed = time;
            Color blurTime = time;
            revealed.a = 1;
            blurTime.a = blur;

            for (int y = 0; y < originalSize; y++)
            {
                for (int x = 0; x < originalSize; x++)
                {
                    int index = x + y * originalSize;
                    quad[0] = original[index];
                    quad[1] = x + 1 < originalSize
                        ? original[index + 1]
                        : Color.clear;

                    if (y + 1 < originalSize)
                    {
                        quad[2] = original[originalSize + index];
                        quad[3] = x + 1 < originalSize
                            ? original[originalSize + index + 1]
                            : Color.clear;
                    }
                    else
                    {
                        quad[2] = Color.clear;
                        quad[3] = Color.clear;
                    }

                    int key = (int)(quad[0].a + quad[1].a * 2 + quad[2].a * 4 + quad[3].a * 8);
                    ChooseMethod(key, x, y, quad, revealed, blurTime);
                }
            }

            return scaled;
        }

        private void ChooseMethod(int key, int x, int y, Color[] quad, Color revealed, Color blurTime)
        {
            int index = x * 4 + y * scaledSize * 4;
            operations[key](index, quad, revealed, blurTime);
        }

        private void Scale0(int index, Color[] quad, Color revealed, Color blurTime)
        {
            scaled[index] = quad[0];
            scaled[index + 1] = quad[0];
            scaled[index + 2] = quad[1];
            scaled[index + 3] = quad[1];

            scaled[index + scaledSize] = quad[0];
            scaled[index + scaledSize + 1] = quad[0];
            scaled[index + scaledSize + 2] = quad[1];
            scaled[index + scaledSize + 3] = quad[1];

            scaled[index + scaledSize * 2] = quad[2];
            scaled[index + scaledSize * 2 + 1] = quad[2];
            scaled[index + scaledSize * 2 + 2] = quad[3];
            scaled[index + scaledSize * 2 + 3] = quad[3];

            scaled[index + scaledSize * 3] = quad[2];
            scaled[index + scaledSize * 3 + 1] = quad[2];
            scaled[index + scaledSize * 3 + 2] = quad[3];
            scaled[index + scaledSize * 3 + 3] = quad[3];
        }

        private void Scale1(int index, Color[] quad, Color revealed, Color blurTime)
        {
            scaled[index] = revealed;
            scaled[index + 1] = blurTime;
            scaled[index + 2] = quad[1];
            scaled[index + 3] = quad[1];

            scaled[index + scaledSize] = blurTime;
            scaled[index + scaledSize + 1] = quad[2];
            scaled[index + scaledSize + 2] = quad[1];
            scaled[index + scaledSize + 3] = quad[1];

            scaled[index + scaledSize * 2] = quad[2];
            scaled[index + scaledSize * 2 + 1] = quad[2];
            scaled[index + scaledSize * 2 + 2] = quad[3];
            scaled[index + scaledSize * 2 + 3] = quad[3];

            scaled[index + scaledSize * 3] = quad[2];
            scaled[index + scaledSize * 3 + 1] = quad[2];
            scaled[index + scaledSize * 3 + 2] = quad[3];
            scaled[index + scaledSize * 3 + 3] = quad[3];
        }

        private void Scale2(int index, Color[] quad, Color revealed, Color blurTime)
        {
            scaled[index] = quad[0];
            scaled[index + 1] = quad[0];
            scaled[index + 2] = blurTime;
            scaled[index + 3] = revealed;

            scaled[index + scaledSize] = quad[0];
            scaled[index + scaledSize + 1] = quad[0];
            scaled[index + scaledSize + 2] = quad[3];
            scaled[index + scaledSize + 3] = blurTime;

            scaled[index + scaledSize * 2] = quad[2];
            scaled[index + scaledSize * 2 + 1] = quad[2];
            scaled[index + scaledSize * 2 + 2] = quad[3];
            scaled[index + scaledSize * 2 + 3] = quad[3];

            scaled[index + scaledSize * 3] = quad[2];
            scaled[index + scaledSize * 3 + 1] = quad[2];
            scaled[index + scaledSize * 3 + 2] = quad[3];
            scaled[index + scaledSize * 3 + 3] = quad[3];
        }

        private void Scale3(int index, Color[] quad, Color revealed, Color blurTime)
        {
            scaled[index] = revealed;
            scaled[index + 1] = revealed;
            scaled[index + 2] = revealed;
            scaled[index + 3] = revealed;

            scaled[index + scaledSize] = revealed;
            scaled[index + scaledSize + 1] = revealed;
            scaled[index + scaledSize + 2] = revealed;
            scaled[index + scaledSize + 3] = revealed;

            scaled[index + scaledSize * 2] = quad[2];
            scaled[index + scaledSize * 2 + 1] = quad[2];
            scaled[index + scaledSize * 2 + 2] = quad[3];
            scaled[index + scaledSize * 2 + 3] = quad[3];

            scaled[index + scaledSize * 3] = quad[2];
            scaled[index + scaledSize * 3 + 1] = quad[2];
            scaled[index + scaledSize * 3 + 2] = quad[3];
            scaled[index + scaledSize * 3 + 3] = quad[3];
        }

        private void Scale4(int index, Color[] quad, Color revealed, Color blurTime)
        {
            scaled[index] = quad[0];
            scaled[index + 1] = quad[0];
            scaled[index + 2] = quad[1];
            scaled[index + 3] = quad[1];

            scaled[index + scaledSize] = quad[0];
            scaled[index + scaledSize + 1] = quad[0];
            scaled[index + scaledSize + 2] = quad[1];
            scaled[index + scaledSize + 3] = quad[1];

            scaled[index + scaledSize * 2] = blurTime;
            scaled[index + scaledSize * 2 + 1] = quad[0];
            scaled[index + scaledSize * 2 + 2] = quad[3];
            scaled[index + scaledSize * 2 + 3] = quad[3];

            scaled[index + scaledSize * 3] = revealed;
            scaled[index + scaledSize * 3 + 1] = blurTime;
            scaled[index + scaledSize * 3 + 2] = quad[3];
            scaled[index + scaledSize * 3 + 3] = quad[3];
        }

        private void Scale5(int index, Color[] quad, Color revealed, Color blurTime)
        {
            scaled[index] = revealed;
            scaled[index + 1] = revealed;
            scaled[index + 2] = quad[1];
            scaled[index + 3] = quad[1];

            scaled[index + scaledSize] = revealed;
            scaled[index + scaledSize + 1] = revealed;
            scaled[index + scaledSize + 2] = quad[1];
            scaled[index + scaledSize + 3] = quad[1];

            scaled[index + scaledSize * 2] = revealed;
            scaled[index + scaledSize * 2 + 1] = revealed;
            scaled[index + scaledSize * 2 + 2] = quad[3];
            scaled[index + scaledSize * 2 + 3] = quad[3];

            scaled[index + scaledSize * 3] = revealed;
            scaled[index + scaledSize * 3 + 1] = revealed;
            scaled[index + scaledSize * 3 + 2] = quad[3];
            scaled[index + scaledSize * 3 + 3] = quad[3];
        }

        private void Scale6(int index, Color[] quad, Color revealed, Color blurTime)
        {
            scaled[index] = quad[0];
            scaled[index + 1] = quad[0];
            scaled[index + 2] = blurTime;
            scaled[index + 3] = revealed;

            scaled[index + scaledSize] = quad[0];
            scaled[index + scaledSize + 1] = quad[0];
            scaled[index + scaledSize + 2] = quad[1];
            scaled[index + scaledSize + 3] = blurTime;

            scaled[index + scaledSize * 2] = blurTime;
            scaled[index + scaledSize * 2 + 1] = quad[0];
            scaled[index + scaledSize * 2 + 2] = quad[3];
            scaled[index + scaledSize * 2 + 3] = quad[3];

            scaled[index + scaledSize * 3] = revealed;
            scaled[index + scaledSize * 3 + 1] = blurTime;
            scaled[index + scaledSize * 3 + 2] = quad[3];
            scaled[index + scaledSize * 3 + 3] = quad[3];
        }

        private void Scale7(int index, Color[] quad, Color revealed, Color blurTime)
        {
            scaled[index] = revealed;
            scaled[index + 1] = revealed;
            scaled[index + 2] = revealed;
            scaled[index + 3] = revealed;

            scaled[index + scaledSize] = revealed;
            scaled[index + scaledSize + 1] = revealed;
            scaled[index + scaledSize + 2] = revealed;
            scaled[index + scaledSize + 3] = revealed;

            scaled[index + scaledSize * 2] = revealed;
            scaled[index + scaledSize * 2 + 1] = revealed;
            scaled[index + scaledSize * 2 + 2] = revealed;
            scaled[index + scaledSize * 2 + 3] = blurTime;

            scaled[index + scaledSize * 3] = revealed;
            scaled[index + scaledSize * 3 + 1] = revealed;
            scaled[index + scaledSize * 3 + 2] = blurTime;
            scaled[index + scaledSize * 3 + 3] = quad[3];
        }

        private void Scale8(int index, Color[] quad, Color revealed, Color blurTime)
        {
            scaled[index] = quad[0];
            scaled[index + 1] = quad[0];
            scaled[index + 2] = quad[1];
            scaled[index + 3] = quad[1];

            scaled[index + scaledSize] = quad[0];
            scaled[index + scaledSize + 1] = quad[0];
            scaled[index + scaledSize + 2] = quad[1];
            scaled[index + scaledSize + 3] = quad[1];

            scaled[index + scaledSize * 2] = quad[2];
            scaled[index + scaledSize * 2 + 1] = quad[2];
            scaled[index + scaledSize * 2 + 2] = quad[1];
            scaled[index + scaledSize * 2 + 3] = blurTime;

            scaled[index + scaledSize * 3] = quad[2];
            scaled[index + scaledSize * 3 + 1] = quad[2];
            scaled[index + scaledSize * 3 + 2] = blurTime;
            scaled[index + scaledSize * 3 + 3] = revealed;
        }

        private void Scale9(int index, Color[] quad, Color revealed, Color blurTime)
        {
            scaled[index] = revealed;
            scaled[index + 1] = blurTime;
            scaled[index + 2] = quad[1];
            scaled[index + 3] = quad[1];

            scaled[index + scaledSize] = blurTime;
            scaled[index + scaledSize + 1] = quad[2];
            scaled[index + scaledSize + 2] = quad[1];
            scaled[index + scaledSize + 3] = quad[1];

            scaled[index + scaledSize * 2] = quad[2];
            scaled[index + scaledSize * 2 + 1] = quad[2];
            scaled[index + scaledSize * 2 + 2] = quad[1];
            scaled[index + scaledSize * 2 + 3] = blurTime;

            scaled[index + scaledSize * 3] = quad[2];
            scaled[index + scaledSize * 3 + 1] = quad[2];
            scaled[index + scaledSize * 3 + 2] = blurTime;
            scaled[index + scaledSize * 3 + 3] = revealed;
        }

        private void Scale10(int index, Color[] quad, Color revealed, Color blurTime)
        {
            scaled[index] = quad[0];
            scaled[index + 1] = quad[0];
            scaled[index + 2] = revealed;
            scaled[index + 3] = revealed;

            scaled[index + scaledSize] = quad[0];
            scaled[index + scaledSize + 1] = quad[0];
            scaled[index + scaledSize + 2] = revealed;
            scaled[index + scaledSize + 3] = revealed;

            scaled[index + scaledSize * 2] = quad[2];
            scaled[index + scaledSize * 2 + 1] = quad[2];
            scaled[index + scaledSize * 2 + 2] = revealed;
            scaled[index + scaledSize * 2 + 3] = revealed;

            scaled[index + scaledSize * 3] = quad[2];
            scaled[index + scaledSize * 3 + 1] = quad[2];
            scaled[index + scaledSize * 3 + 2] = revealed;
            scaled[index + scaledSize * 3 + 3] = revealed;
        }

        private void Scale11(int index, Color[] quad, Color revealed, Color blurTime)
        {
            scaled[index] = revealed;
            scaled[index + 1] = revealed;
            scaled[index + 2] = revealed;
            scaled[index + 3] = revealed;

            scaled[index + scaledSize] = revealed;
            scaled[index + scaledSize + 1] = revealed;
            scaled[index + scaledSize + 2] = revealed;
            scaled[index + scaledSize + 3] = revealed;

            scaled[index + scaledSize * 2] = blurTime;
            scaled[index + scaledSize * 2 + 1] = revealed;
            scaled[index + scaledSize * 2 + 2] = revealed;
            scaled[index + scaledSize * 2 + 3] = revealed;

            scaled[index + scaledSize * 3] = quad[2];
            scaled[index + scaledSize * 3 + 1] = blurTime;
            scaled[index + scaledSize * 3 + 2] = revealed;
            scaled[index + scaledSize * 3 + 3] = revealed;
        }

        private void Scale12(int index, Color[] quad, Color revealed, Color blurTime)
        {
            scaled[index] = quad[0];
            scaled[index + 1] = quad[0];
            scaled[index + 2] = quad[1];
            scaled[index + 3] = quad[1];

            scaled[index + scaledSize] = quad[0];
            scaled[index + scaledSize + 1] = quad[0];
            scaled[index + scaledSize + 2] = quad[1];
            scaled[index + scaledSize + 3] = quad[1];

            scaled[index + scaledSize * 2] = revealed;
            scaled[index + scaledSize * 2 + 1] = revealed;
            scaled[index + scaledSize * 2 + 2] = revealed;
            scaled[index + scaledSize * 2 + 3] = revealed;

            scaled[index + scaledSize * 3] = revealed;
            scaled[index + scaledSize * 3 + 1] = revealed;
            scaled[index + scaledSize * 3 + 2] = revealed;
            scaled[index + scaledSize * 3 + 3] = revealed;
        }

        private void Scale13(int index, Color[] quad, Color revealed, Color blurTime)
        {
            scaled[index] = revealed;
            scaled[index + 1] = revealed;
            scaled[index + 2] = blurTime;
            scaled[index + 3] = quad[1];

            scaled[index + scaledSize] = revealed;
            scaled[index + scaledSize + 1] = revealed;
            scaled[index + scaledSize + 2] = revealed;
            scaled[index + scaledSize + 3] = blurTime;

            scaled[index + scaledSize * 2] = revealed;
            scaled[index + scaledSize * 2 + 1] = revealed;
            scaled[index + scaledSize * 2 + 2] = revealed;
            scaled[index + scaledSize * 2 + 3] = revealed;

            scaled[index + scaledSize * 3] = revealed;
            scaled[index + scaledSize * 3 + 1] = revealed;
            scaled[index + scaledSize * 3 + 2] = revealed;
            scaled[index + scaledSize * 3 + 3] = revealed;
        }

        private void Scale14(int index, Color[] quad, Color revealed, Color blurTime)
        {
            scaled[index] = quad[0];
            scaled[index + 1] = blurTime;
            scaled[index + 2] = revealed;
            scaled[index + 3] = revealed;

            scaled[index + scaledSize] = blurTime;
            scaled[index + scaledSize + 1] = revealed;
            scaled[index + scaledSize + 2] = revealed;
            scaled[index + scaledSize + 3] = revealed;

            scaled[index + scaledSize * 2] = revealed;
            scaled[index + scaledSize * 2 + 1] = revealed;
            scaled[index + scaledSize * 2 + 2] = revealed;
            scaled[index + scaledSize * 2 + 3] = revealed;

            scaled[index + scaledSize * 3] = revealed;
            scaled[index + scaledSize * 3 + 1] = revealed;
            scaled[index + scaledSize * 3 + 2] = revealed;
            scaled[index + scaledSize * 3 + 3] = revealed;
        }

        private void Scale15(int index, Color[] quad, Color revealed, Color blurTime)
        {
            scaled[index] = revealed;
            scaled[index + 1] = revealed;
            scaled[index + 2] = revealed;
            scaled[index + 3] = revealed;

            scaled[index + scaledSize] = revealed;
            scaled[index + scaledSize + 1] = revealed;
            scaled[index + scaledSize + 2] = revealed;
            scaled[index + scaledSize + 3] = revealed;

            scaled[index + scaledSize * 2] = revealed;
            scaled[index + scaledSize * 2 + 1] = revealed;
            scaled[index + scaledSize * 2 + 2] = revealed;
            scaled[index + scaledSize * 2 + 3] = revealed;

            scaled[index + scaledSize * 3] = revealed;
            scaled[index + scaledSize * 3 + 1] = revealed;
            scaled[index + scaledSize * 3 + 2] = revealed;
            scaled[index + scaledSize * 3 + 3] = revealed;
        }
    }
}