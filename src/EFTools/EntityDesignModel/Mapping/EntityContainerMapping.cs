// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Mapping
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class EntityContainerMapping : EFElement
    {
        internal static readonly string ElementName = "EntityContainerMapping";
        internal static readonly string AttributeCdmEntityContainer = "CdmEntityContainer";
        internal static readonly string AttributeStorageEntityContainer = "StorageEntityContainer";
        internal static readonly string AttributeGenerateUpdateViews = "GenerateUpdateViews";

        private readonly List<EntitySetMapping> _entitySetMappings = new List<EntitySetMapping>();
        private readonly List<AssociationSetMapping> _associationSetMappings = new List<AssociationSetMapping>();
        private readonly List<FunctionImportMapping> _functionImportMappings = new List<FunctionImportMapping>();
        private SingleItemBinding<ConceptualEntityContainer> _cdmEntityContainer;
        private SingleItemBinding<StorageEntityContainer> _storageEntityContainer;
        private DefaultableValue<bool> _generateUpdateViewsAttr;

        internal EntityContainerMapping(EFElement parent, XElement element)
            : base(parent, element)
        {
            Debug.Assert(parent is MappingModel, "parent should be a MappingModel");
        }

        /// <summary>
        ///     A bindable reference to the EntityContainer in the C-Space
        /// </summary>
        internal SingleItemBinding<ConceptualEntityContainer> CdmEntityContainer
        {
            get
            {
                if (_cdmEntityContainer == null)
                {
                    _cdmEntityContainer = new SingleItemBinding<ConceptualEntityContainer>(
                        this,
                        AttributeCdmEntityContainer,
                        null);
                }
                return _cdmEntityContainer;
            }
        }

        /// <summary>
        ///     A bindable reference to the EntityContainer in the S-Space
        /// </summary>
        internal SingleItemBinding<StorageEntityContainer> StorageEntityContainer
        {
            get
            {
                if (_storageEntityContainer == null)
                {
                    _storageEntityContainer = new SingleItemBinding<StorageEntityContainer>(
                        this,
                        AttributeStorageEntityContainer,
                        null);
                }
                return _storageEntityContainer;
            }
        }

        /// <summary>
        ///     Manages the content of the GenerateUpdateViews attribute
        /// </summary>
        internal DefaultableValue<bool> GenerateUpdateViews
        {
            get
            {
                if (_generateUpdateViewsAttr == null)
                {
                    _generateUpdateViewsAttr = new GenerateUpdateViewsDefaultableValue(this);
                }
                return _generateUpdateViewsAttr;
            }
        }

        private class GenerateUpdateViewsDefaultableValue : DefaultableValue<bool>
        {
            internal GenerateUpdateViewsDefaultableValue(EntityContainerMapping parent)
                : base(parent, AttributeGenerateUpdateViews)
            {
            }

            internal override string AttributeName
            {
                get { return AttributeGenerateUpdateViews; }
            }

            public override bool DefaultValue
            {
                get { return true; }
            }
        }

        internal void AddEntitySetMapping(EntitySetMapping esm)
        {
            _entitySetMappings.Add(esm);
        }

        internal IList<EntitySetMapping> EntitySetMappings()
        {
            return _entitySetMappings.AsReadOnly();
        }

        internal void AddAssociationSetMapping(AssociationSetMapping asm)
        {
            _associationSetMappings.Add(asm);
        }

        internal IList<AssociationSetMapping> AssociationSetMappings()
        {
            return _associationSetMappings.AsReadOnly();
        }

        internal void AddFunctionImportMapping(FunctionImportMapping fim)
        {
            _functionImportMappings.Add(fim);
        }

        internal IList<FunctionImportMapping> FunctionImportMappings()
        {
            return _functionImportMappings.AsReadOnly();
        }

        // we unfortunately get a warning from the compiler when we use the "base" keyword in "iterator" types generated by using the
        // "yield return" keyword.  By adding this method, I was able to get around this.  Unfortunately, I wasn't able to figure out
        // a way to implement this once and have derived classes share the implementation (since the "base" keyword is resolved at 
        // compile-time and not at runtime.
        private IEnumerable<EFObject> BaseChildren
        {
            get { return base.Children; }
        }

        internal override IEnumerable<EFObject> Children
        {
            get
            {
                foreach (var efobj in BaseChildren)
                {
                    yield return efobj;
                }

                foreach (var child in EntitySetMappings())
                {
                    yield return child;
                }

                foreach (var child in AssociationSetMappings())
                {
                    yield return child;
                }

                foreach (var child in FunctionImportMappings())
                {
                    yield return child;
                }

                yield return CdmEntityContainer;
                yield return StorageEntityContainer;
                yield return GenerateUpdateViews;
            }
        }

        protected override void OnChildDeleted(EFContainer efContainer)
        {
            var child1 = efContainer as EntitySetMapping;
            if (child1 != null)
            {
                _entitySetMappings.Remove(child1);
                return;
            }

            var child2 = efContainer as AssociationSetMapping;
            if (child2 != null)
            {
                _associationSetMappings.Remove(child2);
                return;
            }

            var child3 = efContainer as FunctionImportMapping;
            if (child3 != null)
            {
                _functionImportMappings.Remove(child3);
                return;
            }

            base.OnChildDeleted(efContainer);
        }

#if DEBUG
        internal override ICollection<string> MyAttributeNames()
        {
            var s = base.MyAttributeNames();
            s.Add(AttributeCdmEntityContainer);
            s.Add(AttributeStorageEntityContainer);
            s.Add(AttributeGenerateUpdateViews);
            return s;
        }
#endif

#if DEBUG
        internal override ICollection<string> MyChildElementNames()
        {
            var s = base.MyChildElementNames();
            s.Add(EntitySetMapping.ElementName);
            s.Add(AssociationSetMapping.ElementName);
            s.Add(FunctionImportMapping.ElementName);
            return s;
        }
#endif

        protected override void PreParse()
        {
            Debug.Assert(State != EFElementState.Parsed, "this object should not already be in the parsed state");

            ClearEFObject(_cdmEntityContainer);
            _cdmEntityContainer = null;

            ClearEFObject(_storageEntityContainer);
            _storageEntityContainer = null;

            ClearEFObject(_generateUpdateViewsAttr);
            _generateUpdateViewsAttr = null;

            ClearEFObjectCollection(_entitySetMappings);
            ClearEFObjectCollection(_associationSetMappings);
            ClearEFObjectCollection(_functionImportMappings);
            base.PreParse();
        }

        private string DisplayNameInternal(bool localize)
        {
            string resource;
            if (localize)
            {
                resource = Resources.MappingModel_EntityContainerMappingDisplayName;
            }
            else
            {
                resource = "{0} <==> {1}";
            }

            return string.Format(
                CultureInfo.CurrentCulture,
                resource,
                CdmEntityContainer.RefName,
                StorageEntityContainer.RefName);
        }

        internal override string DisplayName
        {
            get { return DisplayNameInternal(true); }
        }

        internal override string NonLocalizedDisplayName
        {
            get { return DisplayNameInternal(false); }
        }

        internal override bool ParseSingleElement(ICollection<XName> unprocessedElements, XElement elem)
        {
            if (elem.Name.LocalName == EntitySetMapping.ElementName)
            {
                var esm = new EntitySetMapping(this, elem);
                _entitySetMappings.Add(esm);
                esm.Parse(unprocessedElements);
            }
            else if (elem.Name.LocalName == AssociationSetMapping.ElementName)
            {
                var asm = new AssociationSetMapping(this, elem);
                _associationSetMappings.Add(asm);
                asm.Parse(unprocessedElements);
            }
            else if (elem.Name.LocalName == FunctionImportMapping.ElementName)
            {
                var fim = new FunctionImportMapping(this, elem);
                _functionImportMappings.Add(fim);
                fim.Parse(unprocessedElements);
            }
            else
            {
                return base.ParseSingleElement(unprocessedElements, elem);
            }
            return true;
        }

        protected override void DoResolve(EFArtifactSet artifactSet)
        {
            CdmEntityContainer.Rebind();
            StorageEntityContainer.Rebind();

            // its not resolved unless we can resolve both sides
            if (CdmEntityContainer.Status == BindingStatus.Known
                && StorageEntityContainer.Status == BindingStatus.Known)
            {
                State = EFElementState.Resolved;
            }
        }
    }
}
