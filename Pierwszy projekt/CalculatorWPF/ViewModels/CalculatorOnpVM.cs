﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UtilitiesWpf;

namespace CalculatorWPF.ViewModels
{
    enum Command
    {
        Number,
        ArithmeticOperations,
        OpenParenthesis,
        CloseParenthesis,
        Function
    }

    class CalculatorOnpVM : ObserverVM, ICalculatorOnpVM
    {
        private Stack<Command> stackOfCommands = new Stack<Command>();
        private Dictionary<Command, int> howManyCharsInCommand = new Dictionary<Command, int>()
        {
            {Command.ArithmeticOperations, 3 }, {Command.CloseParenthesis, 2},
            {Command.OpenParenthesis, 2}, {Command.Number, 1}, {Command.Function, 3}
        };

        Dictionary<string, int> operatorPriorityDictonary = new Dictionary<string, int>()
        {
            { "+", 10 }, { "-", 10 }, { "*", 20 }, { "/", 20 }, { "%", 20 },

            //specjalne operatory
            { "(", int.MinValue }, { "neg", int.MinValue }
        };

        List<string> funtionList = new List<string>() { "neg" };

        private int countOfNonCloseParenthesis = 0;

        public bool IsParenthesisAvailable { get; set; } = true;

        public string NameOfViewModel { get; set; } = "Kalkulator ONP";

        private string _showValue;
        public string ShowValue
        {
            get
            {
                return _showValue;
            }
            set
            {
                _showValue = value;
                OnPropertyChanged(nameof(ShowValue));
            }
        }

        private ICommand _numberCommand;
        public ICommand NumberCommand
        {
            get
            {
                if (_numberCommand == null)
                    _numberCommand = new RelayCommand<object>(
                        (object o) =>
                        {
                            ShowValue += o.ToString();
                            stackOfCommands.Push(Command.Number);
                        },
                        o => stackOfCommands.Count == 0
                             || stackOfCommands.Peek() == Command.Number
                             || stackOfCommands.Peek() == Command.OpenParenthesis
                             || stackOfCommands.Peek() == Command.ArithmeticOperations);
                return _numberCommand;
            }
        }

        private ICommand _arithmeticOperationsCommand;
        public ICommand ArithmeticOperationsCommand
        {
            get
            {
                if (_arithmeticOperationsCommand == null)
                    _arithmeticOperationsCommand = new RelayCommand<object>(
                        (object o) =>
                        {
                            ShowValue += " " + o.ToString() + " ";
                            stackOfCommands.Push(Command.ArithmeticOperations);
                        },
                        o => stackOfCommands.Count != 0
                             && (stackOfCommands.Peek() == Command.Number
                                 || stackOfCommands.Peek() == Command.CloseParenthesis)
                        );
                return _arithmeticOperationsCommand;
            }
        }

        private ICommand _openParenthesisOperationsCommand;
        public ICommand OpenParenthesisOperationsCommand
        {
            get
            {
                if (_openParenthesisOperationsCommand == null)
                    _openParenthesisOperationsCommand = new RelayCommand<object>(
                        (object o) =>
                        {
                            ShowValue += o.ToString() + " ";
                            stackOfCommands.Push(Command.OpenParenthesis);
                            countOfNonCloseParenthesis++;
                        },
                        o => stackOfCommands.Count == 0
                             || stackOfCommands.Peek() == Command.ArithmeticOperations
                             || stackOfCommands.Peek() == Command.OpenParenthesis
                        );
                return _openParenthesisOperationsCommand;
            }
        }

        private ICommand _closeParenthesisOperationsCommand;
        public ICommand CloseParenthesisOperationsCommand
        {
            get
            {
                if (_closeParenthesisOperationsCommand == null)
                    _closeParenthesisOperationsCommand = new RelayCommand<object>(
                        (object o) =>
                        {
                            ShowValue += " " + o.ToString();
                            stackOfCommands.Push(Command.CloseParenthesis);
                            countOfNonCloseParenthesis--;
                        },
                        o => stackOfCommands.Count != 0
                             && (stackOfCommands.Peek() == Command.Number
                                 || stackOfCommands.Peek() == Command.CloseParenthesis)
                             && countOfNonCloseParenthesis != 0

                        );
                return _closeParenthesisOperationsCommand;
            }
        }

        private ICommand _equalCommand;
        public ICommand EqualCommand
        {
            get
            {
                if (_equalCommand == null)
                    _equalCommand = new RelayCommand<object>(
                        (object o) =>
                        {
                            ShowValue = CalculateOnp(GenerateOnp(ShowValue));
                        },
                        o => stackOfCommands.Count != 0
                             && (stackOfCommands.Peek() == Command.Number
                                 || stackOfCommands.Peek() == Command.CloseParenthesis)
                             && countOfNonCloseParenthesis == 0);
                return _equalCommand;
            }
        }

        private ICommand _functionCommand;
        public ICommand FunctionCommand
        {
            get
            {
                if (_functionCommand == null)
                    _functionCommand = new RelayCommand<object>(
                        (object o) =>
                        {
                            ShowValue += o.ToString() + " ";
                            stackOfCommands.Push(Command.Function);
                            OpenParenthesisOperationsCommand.Execute("(");
                        },
                        o => stackOfCommands.Count == 0
                             || stackOfCommands.Peek() == Command.ArithmeticOperations
                             || stackOfCommands.Peek() == Command.OpenParenthesis);
                return _functionCommand;
            }
        }

        private ICommand _clearCommand;
        public ICommand ClearCommand
        {
            get
            {
                if (_clearCommand == null)
                    _clearCommand = new RelayCommand<object>(
                        (object o) =>
                        {
                            ShowValue = "";
                            stackOfCommands.Clear();
                            countOfNonCloseParenthesis = 0;
                        });
                return _clearCommand;
            }
        }

        private ICommand _backCommand;
        public ICommand BackCommand
        {
            get
            {
                if (_backCommand == null)
                    _backCommand = new RelayCommand<object>(
                        (object o) =>
                        {
                            if (string.IsNullOrEmpty(ShowValue))
                            {
                                return;
                            }

                            Command lastCommand = stackOfCommands.Pop();
                            if (lastCommand == Command.CloseParenthesis)
                                countOfNonCloseParenthesis++;
                            int countToDelete = howManyCharsInCommand[lastCommand];
                            ShowValue = ShowValue.Remove(ShowValue.Length - countToDelete, countToDelete);

                            if (stackOfCommands.Count != 0 && stackOfCommands.Peek() == Command.Function)
                                BackCommand.Execute(null);
                        },
                        o => stackOfCommands.Count != 0
                        );
                return _backCommand;
            }
        }

        private ICommand _keyDownCommand;
        public ICommand KeyDownCommand
        {
            get
            {
                if (_keyDownCommand == null)
                    _keyDownCommand = new RelayCommand<object>(
                        (object o) =>
                        {
                            KeyEventArgs eventArgs = o as KeyEventArgs;
                            if (eventArgs is not null)
                            {
                                if (eventArgs.Key >= Key.NumPad0 && eventArgs.Key <= Key.NumPad9
                                    && NumberCommand.CanExecute(null))
                                    NumberCommand.Execute(((int)eventArgs.Key - 74).ToString());

                                if (eventArgs.KeyboardDevice.Modifiers == ModifierKeys.None
                                    && eventArgs.Key >= Key.D0
                                    && eventArgs.Key <= Key.D9
                                    && NumberCommand.CanExecute(null))
                                    NumberCommand.Execute(((int)eventArgs.Key - 34).ToString());

                                switch (eventArgs.Key)
                                {
                                    case Key.Add:
                                        if (ArithmeticOperationsCommand.CanExecute(null))
                                            ArithmeticOperationsCommand.Execute("+");
                                        break;
                                    case Key.Subtract:
                                        if (ArithmeticOperationsCommand.CanExecute(null))
                                            ArithmeticOperationsCommand.Execute("-");
                                        break;
                                    case Key.Multiply:
                                        if (ArithmeticOperationsCommand.CanExecute(null))
                                            ArithmeticOperationsCommand.Execute("*");
                                        break;
                                    case Key.Divide:
                                        if (ArithmeticOperationsCommand.CanExecute(null))
                                            ArithmeticOperationsCommand.Execute("/");
                                        break;
                                    case Key.D5:
                                        if (eventArgs.KeyboardDevice.Modifiers == ModifierKeys.Shift
                                            && ArithmeticOperationsCommand.CanExecute(null))
                                            ArithmeticOperationsCommand.Execute("%");
                                        break;
                                    case Key.Return:
                                        if (EqualCommand.CanExecute(null))
                                            EqualCommand.Execute(null);
                                        break;
                                    case Key.Back:
                                        if (BackCommand.CanExecute(null))
                                            BackCommand.Execute(null);
                                        break;
                                    case Key.Delete:
                                        if (ClearCommand.CanExecute(null))
                                            ClearCommand.Execute(null);
                                        break;
                                };
                            }
                        });
                return _keyDownCommand;
            }
        }

        private string CalculateOnp(string onpStr)
        {
            List<string> listOfElements = onpStr.Split(" ").ToList();
            Stack<int> stackOfNumbers = new Stack<int>();

            foreach (string element in listOfElements)
            {
                if (int.TryParse(element, out int number))
                {
                    stackOfNumbers.Push(number);
                }
                else
                {
                    int rightNumber = stackOfNumbers.Pop();

                    int leftNumber = 0;
                    if (!funtionList.Contains(element))
                        leftNumber = stackOfNumbers.Pop();

                    int result = Calculate(leftNumber, rightNumber, element);

                    stackOfNumbers.Push(result);
                }
            }

            return stackOfNumbers.Pop().ToString();
        }

        private string GenerateOnp(string showValue)
        {
            List<string> outputList = new List<string>();
            Stack<string> operatorsStack = new Stack<string>();

            List<string> listOfElements = showValue.Split(" ").ToList();

            //konwersja showValue na ONP
            foreach (string element in listOfElements)
            {
                if (int.TryParse(element, out _))
                {
                    //dodajemy na listę wyjściową liczbę lub funkcję
                    outputList.Add(element);
                }
                else if (element == "(" || funtionList.Contains(element))
                {
                    operatorsStack.Push(element);
                }
                else if (element == ")")
                {
                    string operatorOnTopInStack;
                    while ((operatorOnTopInStack = operatorsStack.Pop()) != "(")
                    {
                        outputList.Add(operatorOnTopInStack);
                    }
                    if (operatorsStack.Count != 0
                        && funtionList.Contains(operatorsStack.Peek()))
                    {
                        outputList.Add(operatorsStack.Pop());
                    }
                }
                else
                {
                    while (true)
                    {
                        if (operatorsStack.Count == 0)
                            break;

                        string operatorOnTopInStack = operatorsStack.Peek();

                        //if(operatorOnTopInStack ma wyższy lub równy priorytet niż element)
                        if (operatorPriorityDictonary[operatorOnTopInStack] >= operatorPriorityDictonary[element])
                        {
                            operatorOnTopInStack = operatorsStack.Pop();
                            outputList.Add(operatorOnTopInStack);
                        }
                        else
                        {
                            break;
                        }
                    }
                    operatorsStack.Push(element);
                }
            }

            while (operatorsStack.Count != 0)
            {
                string operatorOnTopInStack = operatorsStack.Pop();
                outputList.Add(operatorOnTopInStack);
            }

            return string.Join(" ", outputList);
        }

        private int Calculate(int leftNumber, int rightNumber, string operatorToDo)
        {
            if (operatorToDo == "+")
                return leftNumber + rightNumber;
            else if (operatorToDo == "-")
                return leftNumber - rightNumber;
            else if (operatorToDo == "*")
                return leftNumber * rightNumber;
            else if (operatorToDo == "/")
                return leftNumber / rightNumber;
            else if (operatorToDo == "%")
                return leftNumber % rightNumber;
            else if (operatorToDo == "neg")
                return -rightNumber;

            return 0;
        }
    }
}
