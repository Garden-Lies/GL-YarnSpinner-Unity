/*
Yarn Spinner is licensed to you under the terms found in the file LICENSE.md.
*/

using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Yarn.Markup;

#if USE_TMP
using TMPro;
#else
using TMP_Text = Yarn.Unity.TMPShim;
#endif

#nullable enable

namespace Yarn.Unity
{

    /// <summary>
    /// Allows pausing the current typewrite through [pause/] markers.
    /// </summary>
    public sealed class PauseEventProcessor : IActionMarkupHandler
    {

        public const int DEFAULT_VALUE = 1000;
        public const string ATTRIBUTE_NAME = "pause";

        private Dictionary<int, float> pauses = new();

        public void OnLineDisplayComplete()
        {
            pauses.Clear();
        }

        public void OnLineDisplayBegin(MarkupParseResult line, TMP_Text text)
        {

            pauses = new();
            // grabbing out any pauses inside the line
            foreach (var attribute in line.Attributes)
            {

                if (attribute.Name != ATTRIBUTE_NAME)
                    continue;
                

                int position = ActionMarkupHandlerUtils.NormalizePosition(attribute.Position, line);
                if (attribute.Properties.TryGetValue(ATTRIBUTE_NAME, out MarkupValue value))
                {

                    // depending on the property value we need to take a different path this is because they have made it an integer or a float which are roughly the same.
                    // But they also might have done something weird and we need to handle that
                    switch (value.Type)
                    {
                        case MarkupValueType.Integer:
                            pauses.Add(position, value.IntegerValue);
                            break;
                        case MarkupValueType.Float:
                            pauses.Add(position, value.FloatValue * DEFAULT_VALUE);
                            break;
                        default:
                            Debug.LogWarning($"Pause property is of type {value.Type}, which is not allowed. Defaulting to one second.");
                            pauses.Add(position, DEFAULT_VALUE);

                            break;
                    }
                }
                else
                {

                    // they haven't set a duration, so we will instead use the
                    // default of one second
                    pauses.Add(position, DEFAULT_VALUE);
                }
            }
        }

        public void OnPrepareForLine(MarkupParseResult line, TMP_Text text)
        {
            return;
        }

        public async YarnTask OnCharacterWillAppear(int currentCharacterIndex, MarkupParseResult line, CancellationToken cancellationToken)
        {

            if (pauses.TryGetValue(currentCharacterIndex, out var duration))
            {
                await YarnTask.Delay(System.TimeSpan.FromMilliseconds(duration), cancellationToken).SuppressCancellationThrow();
            }
        }

        public void OnLineWillDismiss()
        {
            return;
        }
    }
}
