import { Injectable } from "@angular/core";
import { Subject } from "rxjs";

export interface ToastModel {
  header?: string,
  message: string,
  isError?: boolean,
}

@Injectable({
  providedIn: 'root',
})
export class ToastService {

  private subject = new Subject<ToastModel>();

  get data() { return this.subject.asObservable(); }

  add(data: ToastModel): void {
    this.subject.next(data);
  }
}
