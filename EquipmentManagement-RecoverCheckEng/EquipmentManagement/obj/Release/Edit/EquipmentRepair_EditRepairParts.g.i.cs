﻿#pragma checksum "..\..\..\Edit\EquipmentRepair_EditRepairParts.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "12E7254E8706BE811533A406DE47B32FE291C7246B38AF3F57C1865B0CA1ABD7"
//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

using Component.Controls.GridControl;
using Component.Controls.Print;
using Component.Controls.TabControl;
using Component.Controls.ToolButtons;
using Component.Controls.User;
using Component.Controls.User.GenerateControl;
using Component.Controls.UserScrollViewer;
using Component.ControlsEx;
using Component.Converters;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace EquipmentManagement.Edit {
    
    
    /// <summary>
    /// EquipmentRepair_EditRepairParts
    /// </summary>
    public partial class EquipmentRepair_EditRepairParts : Component.ControlsEx.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 8 "..\..\..\Edit\EquipmentRepair_EditRepairParts.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid grid;
        
        #line default
        #line hidden
        
        
        #line 24 "..\..\..\Edit\EquipmentRepair_EditRepairParts.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Component.ControlsEx.TextBoxQuery txtItem;
        
        #line default
        #line hidden
        
        
        #line 27 "..\..\..\Edit\EquipmentRepair_EditRepairParts.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Component.ControlsEx.TextBox txtItemSpecification;
        
        #line default
        #line hidden
        
        
        #line 30 "..\..\..\Edit\EquipmentRepair_EditRepairParts.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Component.ControlsEx.FloatTextBox txtQty;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/EquipmentManagement;component/edit/equipmentrepair_editrepairparts.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Edit\EquipmentRepair_EditRepairParts.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.grid = ((System.Windows.Controls.Grid)(target));
            return;
            case 2:
            this.txtItem = ((Component.ControlsEx.TextBoxQuery)(target));
            
            #line 24 "..\..\..\Edit\EquipmentRepair_EditRepairParts.xaml"
            this.txtItem.SelectedIndexChanged += new System.Windows.RoutedEventHandler(this.txtItem_SelectedIndexChanged);
            
            #line default
            #line hidden
            return;
            case 3:
            this.txtItemSpecification = ((Component.ControlsEx.TextBox)(target));
            return;
            case 4:
            this.txtQty = ((Component.ControlsEx.FloatTextBox)(target));
            return;
            case 5:
            
            #line 35 "..\..\..\Edit\EquipmentRepair_EditRepairParts.xaml"
            ((Component.ControlsEx.CircleImageButton)(target)).Click += new System.Windows.RoutedEventHandler(this.CircleImageButton_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

