﻿
using System;
using ActionStreetMap.Infrastructure.Reactive;

namespace ActionStreetMap.Infrastructure.Utilities
{
    /// <summary> Command line command. </summary>
    public interface ICommand
    {
        /// <summary> Gets unique name of command. </summary>
        string Name { get; }

        /// <summary> Gets description of command. </summary>
        string Description { get; }

        /// <summary> Executes command.</summary>
        /// <param name="args">Argument list.</param>
        /// <returns>Output string.</returns>
        IObservable<string> Execute(params string[] args);
    }

    /// <summary> Provides the way to create command with given lambda to execute. </summary>
    public class Command : ICommand
    {
        private readonly Func<string[], string> _functor;

        /// <inheritdoc />
        public string Name { get; private set; }

        /// <inheritdoc />
        public string Description { get; private set; }

        /// <summary> Creates instance of <see cref="Command"/>. </summary>
        /// <param name="name">Name of command.</param>
        /// <param name="description">Description of command.</param>
        /// <param name="functor">Func to be executed.</param>
        public Command(string name, string description, Func<string[], string> functor)
        {
            Name = name;
            Description = description;
            _functor = functor;
        }

        /// <inheritdoc />
        public IObservable<string> Execute(params string[] args)
        {
            return Observable.Create<string>(o =>
            {
                var result = _functor.Invoke(args);
                o.OnNext(result);
                o.OnCompleted();
                return Disposable.Empty;
            });
        }
    }
}
