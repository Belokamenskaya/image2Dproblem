#version 430
in vec3 texCoord;
layout (rgba32ui, binding = 0) uniform uimage2D inputImage;
/*Будет содержать значение, полученное в результате интерполяции цветов трех вершин треугольника.*/
out vec4 FragColor;

void main()
{
	ivec2 coord = ivec2(texCoord.x, texCoord.y);
	vec4 color = imageLoad(inputImage, coord); 
	FragColor = vec4(color.xyz, 1.0);
}