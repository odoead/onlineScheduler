import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, NgModel, NgModelGroup, Validators } from '@angular/forms';
import { TimeSpanService } from '../../global/services/timeSpanService';
import { MAT_DIALOG_DATA, MatDialog, MatDialogActions, MatDialogContent, MatDialogRef } from '@angular/material/dialog';
import { AddProductComponent } from '../../company/add-product/add-product.component';
import { Product } from '../../global/models/product';
import { CreateBooking } from '../../global/models/createBooking';
import {  BookingService } from '../../global/services/booking.service';
import { EmptyWindow } from '../../global/models/emptyWindow';
import { ScheduleService } from '../../global/services/scheduleInterval.service';
import { MatDatepicker, MatDatepickerContent, MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormField, MatFormFieldModule, MatLabel } from '@angular/material/form-field';
import {MatRadioButton, MatRadioGroup, MatRadioModule} from '@angular/material/radio';
import { CommonModule } from '@angular/common';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-book-product',
  standalone: true,
  imports: [CommonModule,
    MatFormFieldModule,
    MatDatepickerModule,
    MatInputModule,FormsModule,
    MatButtonModule,MatDialogContent,MatDialogActions,
    MatRadioModule,],
  templateUrl: './book-product.component.html',
  styleUrl: './book-product.component.css',
})
export class BookProductComponent {
  form!: FormGroup;
  product!: Product;
  workerId!: string;
  productId!: number;
  selectedDate: Date | null = null;
  availableSlots: EmptyWindow[] = [];
  selectedStartTime: number = 0;
  selectedEndTime: number = 0;
  selectedSlot: EmptyWindow | null = null;

  constructor(
    private fb: FormBuilder,
    private bookingService: BookingService,
    private timespanService: TimeSpanService,
    private sceduleIntervalService: ScheduleService,
    public dialogRef: MatDialogRef<AddProductComponent>,
    @Inject(MAT_DIALOG_DATA)
    public data: { product: Product; productId: number; workerId: string }
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      selectedDate: ['', Validators.required],
      selectedSlot: ['', Validators.required],
    });
    this.product = this.data.product;
    this.productId = this.data.productId;
    this.workerId = this.data.workerId;

    this.form = this.fb.group({
      bookingTimeLOC: ['', Validators.required],
    });
  }

  onDateChange(): void {
    if (this.selectedDate) {
      this.sceduleIntervalService.getEmptyScheduleTimeByDate(this.workerId,this.selectedDate.toString(),false)
      .subscribe((windows) => {this.availableSlots = windows.filter((w) =>
              w.EndTime.hr * 60 + w.EndTime.min -(w.BeginTime.hr * 60 + w.BeginTime.min) >=this.timespanService.getTimeSpanMinuteDuration(this.product.Duration)
            );
        });
        
    }
  }

 

  save(): void {
    if (this.form.valid) {
      const formValue = this.form.value;

      const bookingData: CreateBooking = {
        BookingTimeLOC: new Date(this.selectedDate!.setHours(0, this.selectedStartTime)),//minutes automatically convert to hr if >60 
        Duration: `${this.data.product.Duration.hr}:${this.data.product.Duration.min}:00`,
        WorkerId: this.data.workerId,
        ProductId: this.data.productId, 
      };

      this.bookingService.addBooking(bookingData).subscribe({
        next: (productId: number) => {
          alert(`Product created successfully with ID: ${productId}`);
          this.dialogRef.close(true);
        },
        error: () => {
          alert('Failed to create product');
        },
      });
    }
  }

  cancel(): void {
    this.dialogRef.close(false); 
  }
}
