using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AI.BehaviorTree
{
    public class Node
    {
        public enum Status { Success, Running, Failure };
        public readonly string name;
        public readonly int priority;
        public readonly List<Node> children = new();
        protected int currentIndex;

        public Node CurrentChild { get { return children[currentIndex]; } }

        public Node(string name="Node", int priority=0)
        {
            this.name = name;
            this.priority = priority;
        }

        public void AddChild(Node child) => children.Add(child);
        public virtual Status Process() => children[currentIndex].Process();
        public virtual void Reset()
        {
            currentIndex = 0;
            foreach(Node child in children)
            {
                child.Reset();
            }
        }
    }

    public class BehaviorTree : Node
    {
        public BehaviorTree(string name, int priority=0) : base(name, priority) { }
        public override Status Process()
        {
            while(currentIndex < children.Count)
            {
                Status status = children[currentIndex].Process();
                if(status != Status.Success)
                {
                    return status;
                }
                currentIndex++;

            }
            return Status.Success;
        }
    }

    public class Leaf : Node
    {
        public readonly IStrategy strategy;

        public Leaf(string name, IStrategy strategy, int priority=0) : base(name, priority)
        {
            this.strategy = strategy;
        }

        public override Status Process() => strategy.Process();
        public override void Reset() => strategy.Reset();
    }

    public class Sequence : Node
    {
        public Sequence(string name, int priority = 0) : base(name, priority) { }

        public override Status Process()
        {
            if(currentIndex < children.Count)
            {
                switch(children[currentIndex].Process())
                {
                    case Status.Running:
                        return Status.Running;
                    case Status.Failure:
                        Reset();
                        return Status.Failure;
                    default:
                        currentIndex++;
                        return currentIndex == children.Count ? Status.Success : Status.Running;
                }
            }

            Reset();
            return Status.Success;
        }
    }

    public class Selector : Node
    {
        public Selector(string name, int priority = 0) : base(name, priority) { }

        public override Status Process()
        {
            if(currentIndex < children.Count)
            {
                switch(children[currentIndex].Process())
                {
                    case Status.Running:
                        return Status.Running;
                    case Status.Success:
                        Reset();
                        return Status.Success;
                    default:
                        currentIndex++;
                        return Status.Running;
                }
            }

            Reset();
            return Status.Failure;
        }
    }

    public class PrioritySelector : Selector
    {
        List<Node> sortedChildren;
        List<Node> SortedChildren => sortedChildren ?? SortChildren();
        protected virtual List<Node> SortChildren() => children.OrderByDescending(child => child.priority).ToList();

        public PrioritySelector(string name, int priority = 0) : base(name, priority) { }

        public override Status Process()
        {
            foreach(Node child in SortedChildren)
            {
                switch(child.Process())
                {
                    case Status.Running:
                        return Status.Running;
                    case Status.Success:
                        return Status.Success;
                    default:
                        continue;
                }
            }

            return Status.Failure;
        }
    }

    public class Inverter : Node
    {
        public Inverter(string name, int priority=0) : base(name, priority) { }

        public override Status Process()
        {
            switch(children[0].Process())
            {
                case Status.Running: 
                    return Status.Running;
                case Status.Failure: 
                    return Status.Success;
                default:
                    return Status.Failure;
            }
        }
    }

    public class UntilFail : Node
    {
        public UntilFail(string name, int priority=0) : base(name, priority) { }

        public override Status Process()
        {
            if(children[0].Process() == Status.Failure)
            {
                Reset();
                return Status.Failure;
            }

            return Status.Running;
        }
    }

    public class UntilSuccess : Node
    {
        public UntilSuccess(string name, int priority=0) : base(name, priority) { }

        public override Status Process()
        {
            if(children[0].Process() == Status.Success)
            {
                Reset();
                return Status.Success;
            }

            return Status.Running;
        }
    }

    public class Switch : Node
    {
        readonly Func<bool> predicate;

        public Switch(string name, Func<bool> predicate, int priority=0) : base(name, priority) 
        { 
            this.predicate = predicate;
        }

        public override Status Process()
        {
            if(predicate())
            {
                if(children[0].Process() == Status.Success)
                {
                    Reset();
                    return Status.Success;
                }
                return children[0].Process();
            }
            else
            {
                if(children[1].Process() == Status.Success)
                {
                    Reset();
                    return Status.Success;
                }
                return children[1].Process();
            }
        }

    }
}
