import { Directive, ElementRef, Input, OnInit, Renderer2 } from '@angular/core';
import { AbstractControl, NgControl } from '@angular/forms';

@Directive({
  selector: '[appFormValidation]',
  standalone: true
})
export class FormValidationDirective implements OnInit {
  @Input() customErrorMessages: Record<string, string> = {};
  @Input() showAllErrors = false;
  private defaultErrorMessages: Record<string, string> = {
      required: 'This field is required',
      maxlength: 'Maximum allowed length exceeded',
      minlength: 'Minimum required length not met',
      email: 'Invalid email format',
      pattern: 'Invalid input format',
  };

  constructor(
      private el: ElementRef,
      private control: NgControl,
      private renderer: Renderer2
  ) {}
  ngOnInit(): void {
      if (this.control && this.control.valueChanges) {
          this.control.valueChanges.subscribe(result => {
              this.validateControl();
          });
      }
  }
  private validateControl() {
      const control = this.control.control;
      if (control && control.invalid && (control.dirty || control.touched)) {
          this.showErrors(control);
      } else {
          this.clearErrors();
      }
  }
  private showErrors(control: AbstractControl) {
      const errors = control.errors;
      if (errors) {
          const errorMessages = Object.keys(errors).map(key => {
              if (key === 'serverError') {
                  return errors[key];
              }
              return this.customErrorMessages[key] || this.defaultErrorMessages[key] || `Invalid ${key}`;
          });
          this.clearErrors();
          errorMessages.forEach((message, index) => {
              if (this.showAllErrors || index === 0) {
                  const errorElement = this.renderer.createElement('div');
                  this.renderer.addClass(errorElement, 'error-message');
                  this.renderer.setStyle(errorElement, 'color', 'red');
                  this.renderer.setStyle(errorElement, 'font-size', '0.8em');
                  this.renderer.setProperty(errorElement, 'innerHTML', message);
                  this.renderer.appendChild(this.el.nativeElement.parentElement, errorElement);
              }
          });
      }
  }

  private clearErrors() {
      const errorElements = this.el.nativeElement.parentElement.querySelectorAll('.error-message');
      errorElements.forEach((element: HTMLElement) => {
          this.renderer.removeChild(this.el.nativeElement.parentElement, element);
      });
  }
}
