﻿#pragma checksum "..\..\Scraps.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "F08CC79E17C3EC79E981E146D3A3816826B9A8FD"
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
using EquipmentManagement;
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


namespace EquipmentManagement {
    
    
    /// <summary>
    /// Scraps
    /// </summary>
    public partial class Scraps : Component.Controls.User.UserVendor, System.Windows.Markup.IComponentConnector {
        
        
        #line 9 "..\..\Scraps.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid grid;
        
        #line default
        #line hidden
        
        
        #line 42 "..\..\Scraps.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Component.ControlsEx.TextBox txtEquipmentScrapCode;
        
        #line default
        #line hidden
        
        
        #line 45 "..\..\Scraps.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Component.ControlsEx.ComboBox cmbType;
        
        #line default
        #line hidden
        
        
        #line 48 "..\..\Scraps.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Component.ControlsEx.ComboBox cmbStatus;
        
        #line default
        #line hidden
        
        
        #line 51 "..\..\Scraps.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Component.ControlsEx.TextBox txtEquipmentRepairOutSourcingCode;
        
        #line default
        #line hidden
        
        
        #line 54 "..\..\Scraps.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Component.ControlsEx.TextBox txtEquipmentRepairInCode;
        
        #line default
        #line hidden
        
        
        #line 64 "..\..\Scraps.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Component.ControlsEx.GridControl matrix;
        
        #line default
        #line hidden
        
        
        #line 79 "..\..\Scraps.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Component.ControlsEx.Pager pager;
        
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
            System.Uri resourceLocater = new System.Uri("/EquipmentManagement;component/scraps.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\Scraps.xaml"
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
            this.txtEquipmentScrapCode = ((Component.ControlsEx.TextBox)(target));
            return;
            case 3:
            this.cmbType = ((Component.ControlsEx.ComboBox)(target));
            return;
            case 4:
            this.cmbStatus = ((Component.ControlsEx.ComboBox)(target));
            return;
            case 5:
            this.txtEquipmentRepairOutSourcingCode = ((Component.ControlsEx.TextBox)(target));
            return;
            case 6:
            this.txtEquipmentRepairInCode = ((Component.ControlsEx.TextBox)(target));
            return;
            case 7:
            
            #line 58 "..\..\Scraps.xaml"
            ((Component.ControlsEx.CircleImageButton)(target)).Click += new System.Windows.RoutedEventHandler(this.btnQuery_Click);
            
            #line default
            #line hidden
            return;
            case 8:
            this.matrix = ((Component.ControlsEx.GridControl)(target));
            return;
            case 9:
            
            #line 66 "..\..\Scraps.xaml"
            ((Component.ControlsEx.GridLinkColumn)(target)).LinkClick += new System.Action<object, object>(this.GridLinkColumn_LinkClick);
            
            #line default
            #line hidden
            return;
            case 10:
            this.pager = ((Component.ControlsEx.Pager)(target));
            
            #line 79 "..\..\Scraps.xaml"
            this.pager.UpdateSource += new System.Windows.RoutedEventHandler(this.pager_UpdateSource);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

