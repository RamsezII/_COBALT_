﻿using _COBRA_;
using System;
using UnityEngine;

namespace _COBALT_
{
    partial class Terminal
    {
        char OnValidateStdin(string text, int charIndex, char addedChar)
        {
            Command.Line.ResetHistoryCount();
            flag_stdin.Update(true);

            switch (shell.CurrentStatus.state)
            {
                case CMD_STATES.BLOCKING:
                    return '\0';

                case CMD_STATES.FULLSCREEN_write:
                    {
                        Command.Line line = new(text + addedChar, SIGNAL_FLAGS.STDIN_CHANGE, this);
                        shell.PropagateLine(line);
                    }
                    return addedChar;
            }

            try
            {
                switch (addedChar)
                {
                    case '\t':
                        OnTab(charIndex);
                        return '\0';

                    case '\n':
                        OnSubmit();
                        return '\0';
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
                return '\0';
            }
            return addedChar;
        }
    }
}