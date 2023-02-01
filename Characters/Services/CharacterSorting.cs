﻿using Blish_HUD.Controls.Extern;
using Characters.Res;
using Kenedia.Modules.Characters.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using Kenedia.Modules.Core.Extensions;

namespace Kenedia.Modules.Characters.Services
{
    public enum SortingState
    {
        None,
        MovedToFirst,
        Selected,
        FirstNameFetched,
        Done,
        Canceled,
    }

    public class CharacterSorting
    {
        public CharacterSwapping CharacterSwapping { get; set; }

        private CancellationTokenSource _cancellationTokenSource;
        private List<Character_Model> _models;
        private SortingState _state;
        private string _lastName;
        private int _noNameChange = 0;

        [SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "<Pending>")]
        private int NoNameChange
        {
            get => _noNameChange;
            set
            {
                _noNameChange = value;
                if (_noNameChange > 0)
                {
                    Status = string.Format(strings.FixCharacter_NoChange, _noNameChange);
                }

                if (_noNameChange >= 2)
                {
                    _state = SortingState.Done;
                    Status = strings.Status_Done;
                    AdjustCharacterLogins();
                    Completed?.Invoke(null, null);
                }
            }
        }

        private int _currentIndex = 0;

        public event EventHandler Started;
        public event EventHandler Completed;
        public event EventHandler Finished;
        public event EventHandler StatusChanged;

        private string _status;
        public string Status
        {
            set
            {
                _status = value;
                StatusChanged?.Invoke(null, EventArgs.Empty);
            }
            get => _status;
        }

        public void Cancel()
        {
            _state = SortingState.Canceled;
            _cancellationTokenSource?.Cancel();
            //s_cancellationTokenSource = null;
        }

        public async void Start(IEnumerable<Character_Model> models)
        {
            _state = SortingState.Canceled;
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new();
            _cancellationTokenSource.CancelAfter(180000);

            _models = models.OrderByDescending(e => e.LastLogin).ToList();
            _lastName = string.Empty;
            _state = SortingState.None;
            NoNameChange = 0;

            Started?.Invoke(null, null);
            Status = strings.FixCharacter_Start;
            int i = 0;
            while (_state is not SortingState.Done and not SortingState.Canceled && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                i++;

                try
                {
                    await Run(_cancellationTokenSource.Token);
                }
                catch (TaskCanceledException)
                {

                }
            }

            Finished?.Invoke(null, null);
        }

        private async Task Delay(CancellationToken cancellationToken, int? delay = null, double? partial = null)
        {
            delay ??= Characters.ModuleInstance.Settings.KeyDelay.Value;

            if (delay > 0)
            {
                if(partial != null)
                {
                    delay = delay / 100 * (int)(partial * 100);
                }

                await Task.Delay(delay.Value, cancellationToken);
            }
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            string name;

            if (cancellationToken.IsCancellationRequested) return;

            switch (_state)
            {
                case SortingState.None:
                    await MoveFirst(cancellationToken);
                    await Delay(cancellationToken, 125);
                    break;

                case SortingState.MovedToFirst:
                    name = await FetchName(cancellationToken);
                    if (name == _lastName)
                    {
                        NoNameChange++;
                    }
                    else
                    {
                        NoNameChange = 0;
                    }

                    _lastName = name;
                    break;

                case SortingState.FirstNameFetched:
                    await MoveNext(cancellationToken);

                    name = await FetchName(cancellationToken);
                    if (name == _lastName)
                    {
                        NoNameChange++;
                    }
                    else
                    {
                        NoNameChange = 0;
                    }

                    _lastName = name;

                    await Delay(cancellationToken, 250);
                    break;
            }
        }

        private async Task MoveFirst(CancellationToken cancellationToken)
        {
            Status = strings.CharacterSwap_MoveFirst;
            if (IsTaskCanceled(cancellationToken)) { return; }

            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < _models.Count; i++)
            {
                Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.LEFT, false);
                await Delay(cancellationToken, null, 0.05);

                if (IsTaskCanceled(cancellationToken)) { return; }

                if (stopwatch.ElapsedMilliseconds > 5000)
                {
                    InputService.MouseWiggle();
                    stopwatch.Restart();
                }
            }

            _state = SortingState.MovedToFirst;
            _currentIndex = 0;
        }

        private async Task MoveNext(CancellationToken cancellationToken)
        {
            Status = strings.FixCharacter_MoveNext;
            InputService.MouseWiggle();
            Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.RIGHT, false);
            await Delay(cancellationToken);
            _currentIndex++;
        }

        private (string, int, int, int, bool) GetBestMatch(string name)
        {
            var distances = new List<(string, int, int, int, bool)>();

            Character_Model expectedCharacter = _models.Find(e => e.OrderIndex == _currentIndex);
            string expectedCharacterName = expectedCharacter == null ? name : expectedCharacter.Name;

            foreach (Character_Model c in _models)
            {
                int distance = name.LevenshteinDistance(c.Name);
                int listDistance = c.Position.Difference(_currentIndex);

                distances.Add((c.Name, distance, listDistance, listDistance + distance, expectedCharacter != null && c.Name == expectedCharacter?.Name));
            }

            distances.Sort((a, b) => a.Item4.CompareTo(b.Item4));
            (string, int, int, int, bool)? bestMatch = distances?.FirstOrDefault();

            if (bestMatch != null && bestMatch.HasValue)
            {
                foreach ((string, int, int, int, bool) match in distances.Where(e => e.Item4 == bestMatch.Value.Item4))
                {
                    if (match.Item1 == expectedCharacterName)
                    {
                        return match;
                    }
                }
            }

            return ((string, int, int, int, bool))bestMatch;
        }

        private async Task<string> FetchName(CancellationToken cancellationToken)
        {
            string name = await Characters.ModuleInstance.OCR.Read();

            Status = string.Format(strings.FixCharacter_FetchName, Environment.NewLine, name);

            if(name != null)
            {
                (string, int, int, int, bool) match = GetBestMatch(name);
                Debug.WriteLine($"Best Match for {_currentIndex} is {match.Item1} with {match.Item2} string edits and {match.Item3} list index distance for a total of {match.Item4} differences.");

                //Character_Model c = s_models.Find(e => e.Name == name);
                Character_Model c = _models.Find(e => e.Name == match.Item1);

                if (c != null)
                {
                    c.OrderIndex = _currentIndex;
                }
                else
                {
                    Status = string.Format(strings.CouldNotFindNamedItem, strings.Character, name);
                }

                await Delay(cancellationToken);
            }

            if (_state == SortingState.MovedToFirst)
            {
                _state = SortingState.FirstNameFetched;
            }

            return name;
        }

        private bool IsTaskCanceled(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _state = SortingState.Canceled;
                return true;
            }

            return false;
        }

        private void AdjustCharacterLogins()
        {
            _models = _models.OrderBy(e => e.OrderIndex).ToList();

            bool messedUp = true;

            while (messedUp)
            {
                messedUp = false;

                for (int i = 0; i < _models.Count; i++)
                {
                    Character_Model next = _models.Count > i + 1 ? _models[i + 1] : null;
                    Character_Model current = _models[i];

                    // var nCurr = string.Format("Current: {0} | LastLogin: {1} | More Recent: {2}", current.Name, current.LastLogin, next != null && current.LastLogin <= next.LastLogin);
                    // var nNext = string.Format("Next: {0} | LastLogin: {1} | More Recent: {2}", next != null ? next.Name : "No Next", next != null ? next.LastLogin : "No Next", next != null && current.LastLogin <= next.LastLogin);

                    // Characters.Logger.Debug(nCurr);
                    // Characters.Logger.Debug(nNext + Environment.NewLine);
                    if (next != null && current.LastLogin <= next.LastLogin)
                    {
                        current.LastLogin = next.LastLogin.AddSeconds(1);
                        messedUp = true;
                    }
                }
            }
        }
    }
}