using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Loxonator.Common.Helpers;

namespace Loxonator.Common.Data
{
    public class Node : INotifyPropertyChanged
    {
        private string address = String.Empty;
        private string name = String.Empty;
        private Node parent = null;
        private AsyncObservableCollection<Node> children = new AsyncObservableCollection<Node>();
        private bool? isSensor = null;
        private bool? isActor = null;
        private NodeType? type = null;
        private bool isExpanded = true;
        private bool isSelected = false;

        public string Address
        {
            get { return this.address; }
            set 
            {
                if (this.address != value)
                {
                    this.address = value;
                    this.OnPropertyChanged("Address");
                    this.OnPropertyChanged("DisplayName");
                }
            }
        }

        public string DottedAddress
        {
            get
            {
                if (String.IsNullOrEmpty(this.address))
                    return String.Empty;
                return this.address.Replace('/', '.');
            }
        }

        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.name != value)
                {
                    this.name = value;
                    this.OnPropertyChanged("Name");
                    this.OnPropertyChanged("DisplayName");
                }
            }
        }

        public string DisplayName
        {
            get { return this.ToString();  }
        }

        public Node Parent
        {
            get { return this.parent; }
            set
            {
                if (this.parent != value)
                {
                    if (this.parent != null)
                    {
                        this.PropertyChanged -= this.parent.HandleChildPropertyChanged;
                        this.parent.Children.Remove(this);
                        this.parent.OnPropertyChanged("HasChildren");
                    }
                    this.parent = value;
                    if (this.parent != null)
                    {
                        this.parent.Children.Add(this);
                        this.parent.OnPropertyChanged("HasChildren");
                        this.PropertyChanged += this.parent.HandleChildPropertyChanged;
                    }
                    this.OnPropertyChanged("Parent");
                }
            }
        }

        private void HandleChildPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedNode")
                this.OnPropertyChanged("SelectedNode"); // forward to root
        }

        public ObservableCollection<Node> Children
        {
            get { return this.children; }
        }

        public bool HasChildren
        {
            get { return this.children.Count > 0; }
        }

        public bool IsSensor
        {
            get
            {
                if (this.isSensor != null)
                    return this.isSensor.Value;
                else if (this.parent != null)
                    return this.parent.IsSensor;
                return true;
            }
            set
            {
                if (this.IsSensor != value)
                {
                    if (this.parent != null && this.parent.IsSensor == value)
                        this.isSensor = null;
                    else
                        this.isSensor = value;
                    this.OnPropertyChanged("IsSensor");
                    this.OnPropertyChanged("IsSensorInherited");
                }
            }
        }

        public bool IsSensorInherited
        {
            get { return this.isSensor == null && this.parent != null; }
        }

        public bool IsActor
        {
            get
            {
                if (this.isActor != null)
                    return this.isActor.Value;
                else if (this.parent != null)
                    return this.parent.IsActor;
                return true;
            }
            set
            {
                if (this.IsActor != value)
                {
                    if (this.parent != null && this.parent.IsActor == value)
                        this.isActor = null;
                    else
                        this.isActor = value;
                    this.OnPropertyChanged("IsActor");
                    this.OnPropertyChanged("IsActorInherited");
                }
            }
        }

        public bool IsActorInherited
        {
            get { return this.isActor == null && this.parent != null; }
        }

        public NodeType Type
        {
            get
            {
                if (this.type != null)
                    return this.type.Value;
                else if (this.parent != null)
                    return this.parent.Type;
                return NodeType.EIS1;
            }
            set
            {
                if (this.Type != value)
                {
                    if (this.parent != null && this.parent.Type == value)
                        this.type = null;
                    else
                        this.type = value;
                    this.OnPropertyChanged("Type");
                    this.OnPropertyChanged("IsTypeInherited");
                }
            }
        }

        public IEnumerable<NodeType> PossibleTypes
        {
            get
            {
                return Enum.GetValues(typeof(NodeType))
                    .Cast<NodeType>();
            }
        }


        public bool IsTypeInherited
        {
            get { return this.type == null && this.parent != null; }
        }

        public bool IsLeaf
        {
            get { return this.children.Count == 0; }
        }

        public bool IsExpanded
        {
            get { return this.isExpanded; }
            set
            {
                if (this.isExpanded != value)
                {
                    this.isExpanded = value;
                    this.OnPropertyChanged("IsExpanded");
                }
            }
        }

        public bool IsSelected
        {
            get { return this.isSelected; }
            set
            {
                if (this.isSelected != value)
                {
                    this.isSelected = value;
                    this.OnPropertyChanged("IsSelected");
                    if (this.isSelected)
                        this.OnPropertyChanged("SelectedNode");
                }
            }
        }

        public Node SelectedNode
        {
            get
            {
                if (this.isSelected)
                    return this;
                else
                {
                    foreach (Node child in this.children)
                    {
                        Node selectedChild = child.SelectedNode;
                        if (selectedChild != null)
                            return selectedChild;
                    }
                    return null;
                }
            }
        }

        public IEnumerable<Node> AllLeafs
        {
            get
            {
                if (this.IsLeaf)
                    yield return this;
                else
                {
                    foreach (Node subNode in this.children)
                    {
                        if (subNode.IsLeaf)
                            yield return subNode;
                        else
                        {
                            foreach (Node subLeaf in subNode.AllLeafs)
                                yield return subLeaf;
                        }
                    }
                }
            }
        }

        public Node()
        {
        }

        public Node(string address)
        {
            this.address = address;
        }

        public Node(string address, string name)
            : this(address)
        {
            this.name = name;
        }

        public override string ToString()
        {
            if (String.IsNullOrEmpty(this.Address))
                return this.Name;
            else
                return String.Format("{0} ({1})", this.Name, this.Address);
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
