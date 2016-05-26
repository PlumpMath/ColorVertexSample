#version 420 core

/*
 * OpenGL Programming Guide - Order Independent Transparency Example
 *
 * This is the resolve shader for order independent transparency.
 */

// The per-pixel image containing the head pointers
layout (binding = 0, r32ui) uniform uimage2D head_pointer_image;
// Buffer containing linked lists of fragments
layout (binding = 1, rgba32ui) uniform uimageBuffer list_buffer;

// This is the output color
layout (location = 0) out vec4 color;

// This is the maximum number of overlapping fragments allowed
#define MAX_FRAGMENTS 40

// Temporary array used for sorting fragments
uvec4 fragment_list[MAX_FRAGMENTS];

void main(void)
{
    vec4 final_color = vec4(0.0);

    uint current_index;
    uint fragment_count = 0;

    current_index = imageLoad(head_pointer_image, ivec2(gl_FragCoord).xy).x;

	if (current_index == 0) { final_color.b = 1.0f; }

    while (current_index != 0 && fragment_count < MAX_FRAGMENTS)
    {
        uvec4 fragment = imageLoad(list_buffer, int(current_index));
        fragment_list[fragment_count] = fragment;
        current_index = fragment.x;
        fragment_count++;
    }

    uint i, j;

    if (fragment_count > 1)
    {

        for (i = 0; i < fragment_count - 1; i++)
        {
            for (j = i + 1; j < fragment_count; j++)
            {
                uvec4 fragment1 = fragment_list[i];
                uvec4 fragment2 = fragment_list[j];

                float depth1 = uintBitsToFloat(fragment1.z);
                float depth2 = uintBitsToFloat(fragment2.z);

                if (depth1 < depth2)
                {
                    fragment_list[i] = fragment2;
                    fragment_list[j] = fragment1;
                }
            }
        }

    }

    for (i = 0; i < fragment_count; i++)
    {
        vec4 modulator = unpackUnorm4x8(fragment_list[i].y);

        final_color = mix(final_color, modulator, modulator.a + fragment_list[i].w / 255);
    }
	if (fragment_count == 0) { final_color.g = 1.0f; }
	if (final_color.a <= 0.5) { final_color.a = 0.5; }
	//{ final_color.r = 1.0f; }

    //color = final_color;
	discard;
    // color = vec4(float(fragment_count) / float(MAX_FRAGMENTS));
}
