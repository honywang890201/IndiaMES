﻿#pragma checksum "..\..\..\Edit\Scrap_EditSureComment.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "95524DE84DCB026F63401C652A2194F97FDEA85D42233CFDF83DF1AE6A1B18F6"
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
    /// Scrap_EditSureComment
    /// </summary>
    public partial class Scrap_EditSureComment : Component.ControlsEx.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 8 "..\..\..\Edit\Scrap_EditSureComment.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid grid;
        
        #line default
        #line hidden
        
        
        #line 21 "..\..\..\Edit\Scrap_EditSureComment.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock tbTitle;
        
        #line default
        #line hidden
        
        
        #line 23 "..\..\..\Edit\Scrap_EditSureComment.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock tbComment;
        
        #line default
        #line hidden
        
        
        #line 24 "..\..\..\Edit\Scrap_EditSureComment.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Component.ControlsEx.TextBox txtSureComment;
        
        #line default
        #line hidden
        
        
        #line 28 "..\..\..\Edit\Scrap_EditSureComment.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Component.ControlsEx.CircleImageButton btnSure;
        
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
            System.Uri resourceLocater = new System.Uri("/EquipmentManagement;component/edit/scrap_editsurecomment.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Edit\Scrap_EditSureComment.xaml"
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
            this.tbTitle = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 3:
            this.tbComment = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 4:
            this.txtSureComment = ((Component.ControlsEx.TextBox)(target));
            return;
            case 5:
            this.btnSure = ((Component.ControlsEx.CircleImageButton)(target));
            
            #line 29 "..\..\..\Edit\Scrap_EditSureComment.xaml"
            this.btnSure.Click += new System.Windows.RoutedEventHandler(this.CircleImageButton_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

