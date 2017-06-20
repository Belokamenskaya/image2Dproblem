#version 430
/*Входные атрибуты для вершинного шейдера. Ключевое слово in*/
layout (location = 0) in vec3 VertexPosition;
layout (location = 1) in vec3 TexCoord;

out vec3 texCoord;

void main()
{
	texCoord = TexCoord;
	/*Встроенная выходная переменная*/
	gl_Position = vec4(VertexPosition, 1.0);
}