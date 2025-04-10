﻿using _COBRA_;
using UnityEngine;

namespace _COBALT_
{
    partial class Terminal
    {
        void RefreshStdin()
        {
            CMD_STATUS status = shell.CurrentStatus;

            input_prefixe.input_field.text = status.prefixe;

            Vector2 prefered_dims = input_prefixe.input_field.textComponent.GetPreferredValues(input_prefixe.input_field.text + "_", scrollview.content.rect.width, float.PositiveInfinity);
            line_height = prefered_dims.y;

            if (string.IsNullOrWhiteSpace(input_prefixe.input_field.text))
                prefered_dims.x = 0;

            input_stdin.rT.sizeDelta = new(-prefered_dims.x, 0);

            input_prefixe.AutoSize(false);
            input_stdin.AutoSize(false);

            linter_tmp.text = linter.GetLint(this, input_stdin.input_field.text, out _);

            if (string.IsNullOrWhiteSpace(input_stdin.input_field.text))
            {
                rT_stdin.sizeDelta = new(rT_stdin.sizeDelta.x, scrollview.viewport.rect.height);
                scrollview.content.sizeDelta = new(0, 1 + input_stdout.text_height + input_realtime.text_height + scrollview.viewport.rect.height - line_height);
            }
            else
            {
                float stdin_height = Mathf.Max(input_stdin.text_height, scrollview.viewport.rect.height);

                rT_stdin.sizeDelta = new(rT_stdin.sizeDelta.x, stdin_height);
                scrollview.content.sizeDelta = new(0, 1 + input_stdout.text_height + input_realtime.text_height + stdin_height);
            }

            flag_clampbottom.Update(true);
        }
    }
}