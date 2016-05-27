#version 420 core

layout (early_fragment_tests) in;

layout (binding = 0, r32ui) uniform uimage2D head_pointer_image;
layout (binding = 1, rgba32ui) uniform writeonly uimageBuffer list_buffer;

layout (binding = 0, offset = 0) uniform atomic_uint list_counter;

layout (location = 0) out vec4 color;

in float pass_uv;

uniform sampler2D tex;
uniform float renderingWireframe;
uniform float brightness = 1.0f;
uniform float opacity = 1.0f;

uniform int list_buffer_length;

void main(void)
{
	vec4 surface_color;
	bool discarded = false;
	if (renderingWireframe > 0.0)
	{
		if (0.0 <= pass_uv && pass_uv <= 1.0)
		{
			surface_color = vec4(1, 1, 1, opacity);
		}
		else
		{
			discarded = true;
			discard;
		}
	}
	else
	{
	    if (0.0 <= pass_uv && pass_uv <= 1.0)
		{
			vec4 color = texture(tex, vec2(pass_uv, 0.0)) * brightness;
			color.a = opacity;
			surface_color = color;
		}
		else
		{
			discarded = true;
			discard;
		}
	}

	if (!discarded)
	{
		uint index;
		uint old_head;
		uvec4 item;

		index = atomicCounterIncrement(list_counter);

		if (index < list_buffer_length)
		{
			old_head = imageAtomicExchange(head_pointer_image, ivec2(gl_FragCoord.xy), uint(index));

			item.x = old_head;
			item.y = packUnorm4x8(surface_color);
			item.z = floatBitsToUint(gl_FragCoord.z);
			item.w = 255 / 4;

			imageStore(list_buffer, int(index), item);
		}

		discard;
	}
	//color = surface_color;
}
