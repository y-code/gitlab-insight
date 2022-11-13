import { Component } from "@angular/core";
import { tap } from "rxjs";
import { ToastService } from "./toast.service";

@Component({
  selector: 'app-toasts',
  template: `

    <ngb-toast
      *ngFor="let toast of toasts; index as index"
      [header]="toast.header||''"
      (hide)="toasts.splice(index, 1)">
      <p *ngFor="let line of toast.message">
        {{line}}
      </p>
    </ngb-toast>

  `,
  styles: [`

    :host {
      position: fixed;
      top: 0;
      right: 0;
      margin: 0.5em;
      z-index: 1200;
    }

  `]
})
export class ToastsComponent {

  toasts: Array<{header?: string, message: string[]}> = [];

  constructor(
    private toast: ToastService
  ) {
    this.toast.data.pipe(
      tap(x => {
        this.toasts = [
          ...this.toasts,
          {
            ...x,
            message: !x.message
              ? []
              : typeof(x.message) === 'string'
                ? x.message.split('\n')
                : [x.message],
          },
        ];
      }),
    ).subscribe();
  }
}