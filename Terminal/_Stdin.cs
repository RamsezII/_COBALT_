﻿using _ARK_;
using _COBRA_;
using _UTIL_;
using System;
using UnityEngine;

namespace _COBALT_
{
    partial class Terminal
    {
        public readonly OnValue<KeyCode>
            flag_alt = new(),
            flag_nav_history = new();

        [SerializeField] string stdin_save;
        [SerializeField] int cpl_index;
        [SerializeField] int stdin_frame, tab_frame;

        //--------------------------------------------------------------------------------------------------------------

        void OnAltKey()
        {
            CMD_SIGNALS signal = flag_alt.PullValue switch
            {
                KeyCode.LeftArrow => CMD_SIGNALS.LEFT,
                KeyCode.RightArrow => CMD_SIGNALS.RIGHT,
                KeyCode.UpArrow => CMD_SIGNALS.UP,
                KeyCode.DownArrow => CMD_SIGNALS.DOWN,
                _ => 0,
            };

            if (signal == 0)
                return;

            signal |= CMD_SIGNALS.CPL_ALT;

            flag_stdin.Update(true);

            tab_frame = Time.frameCount;
            stdin_save = input_stdin.input_field.text;

            try
            {
                Command.Line line = new(
                    stdin_frame >= tab_frame ? input_stdin.input_field.text : stdin_save,
                    signal,
                    this,
                    input_stdin.input_field.caretPosition
                    );

                executor.Executate(line);
                stdin_save = line.text;
                input_stdin.input_field.text = line.text;
                input_stdin.input_field.caretPosition = line.cursor_i;
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
            }
        }

        void OnChangeStdin(string text)
        {
            if (tab_frame == Time.frameCount)
                return;

            cpl_index = 0;
            stdin_save = text;
            stdin_frame = Time.frameCount;
            flag_stdin.Update(true);

            if (executor.routine != null)
                switch (executor.routine.Current.state)
                {
                    case CMD_STATES.BLOCKING:
                    case CMD_STATES.FULLSCREEN_readonly:
                        input_stdin.ResetText();
                        break;
                }
        }

        char OnValidateStdin(string text, int charIndex, char addedChar)
        {
            flag_stdin.Update(true);

            if (executor.routine != null)
                switch (executor.routine.Current.state)
                {
                    case CMD_STATES.BLOCKING:
                        return '\0';
                    case CMD_STATES.FULLSCREEN_write:
                        return addedChar;
                }

            Command.Line.ResetHistoryCount();
            switch (addedChar)
            {
                case '\t':
                    tab_frame = Time.frameCount;
                    try
                    {
                        Command.Line line = new(
                            stdin_save,
                            CMD_SIGNALS.CPL_TAB,
                            this,
                            Mathf.Min(stdin_save.Length, charIndex),
                            cpl_index++
                            );

                        executor.Executate(line);
                        input_stdin.input_field.text = line.text;
                        input_stdin.input_field.caretPosition = line.cursor_i;
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e, this);
                    }
                    return '\0';

                case '\n':
                    cpl_index = 0;
                    stdin_save = null;
                    if (executor.routine == null && string.IsNullOrWhiteSpace(input_stdin.input_field.text))
                        isActive.Update(false);
                    else
                        try
                        {
                            if (executor.routine == null || executor.routine.Current.state < CMD_STATES.FULLSCREEN_readonly)
                            {
                                string lint_text = linter.GetLint(executor, input_stdin.input_field.text);
                                Debug.Log(input_prefixe.input_field.text + " " + lint_text, this);
                            }

                            Command.Line line = new(input_stdin.input_field.text, CMD_SIGNALS.CHECK, this);
                            executor.Executate(line);

                            if (executor.error == null)
                            {
                                line = new(input_stdin.input_field.text, CMD_SIGNALS.EXEC, this);
                                bool noRoutine = executor.routine == null;

                                executor.Executate(line);

                                if (noRoutine && executor.error == null)
                                    Command.Line.AddToHistory(line.text);

                                hide_stdout.Update(executor.routine != null && executor.routine.Current.state >= CMD_STATES.FULLSCREEN_readonly);
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e, this);
                        }
                    input_stdin.ResetText();
                    return '\0';
            }
            return addedChar;
        }

        public void RefreshStdin()
        {
            if (executor.routine == null)
                input_prefixe.input_field.text = executor.GetPrefixe();
            else
                input_prefixe.input_field.text = executor.routine.Current.prefixe;

            bool was_interactable = input_stdin.input_field.interactable;

            if (executor.routine == null)
                input_stdin.input_field.interactable = true;
            else
                input_stdin.input_field.interactable = executor.routine.Current.state switch
                {
                    CMD_STATES.BLOCKING => false,
                    CMD_STATES.FULLSCREEN_readonly => false,
                    _ => true,
                };

            if (!was_interactable && input_stdin.input_field.interactable)
                input_stdin.input_field.Select();

            Vector2 prefered_dims = input_prefixe.input_field.textComponent.GetPreferredValues(input_prefixe.input_field.text + "_", scrollview.content.rect.width, float.PositiveInfinity);
            line_height = prefered_dims.y;

            if (string.IsNullOrWhiteSpace(input_prefixe.input_field.text))
                prefered_dims.x = 0;

            input_stdin.rT.sizeDelta = new(-prefered_dims.x, 0);

            input_prefixe.AutoSize(false);
            input_stdin.AutoSize(false);

            linter_tmp.text = linter.GetLint(executor, input_stdin.input_field.text);

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