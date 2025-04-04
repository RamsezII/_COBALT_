﻿using System.Collections.Generic;
using UnityEngine;

namespace _COBALT_
{
    partial class Terminal
    {
        readonly Queue<object> lines = new();

        [SerializeField, TextArea(1, 10)] string stdout;

        const byte max_lines = 250;

        //--------------------------------------------------------------------------------------------------------------

        public void AddLine(in object line)
        {
            lock (lines)
            {
                while (lines.Count >= max_lines)
                    lines.Dequeue();
                lines.Enqueue(line);
                flag_stdout.Update(true);
            }
        }

        public void ClearStdout()
        {
            scrollview.verticalNormalizedPosition = 0;
        }
    }
}