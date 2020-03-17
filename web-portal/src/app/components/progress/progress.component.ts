import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'app-progress',
  templateUrl: './progress.component.html'
})
export class ProgressComponent implements OnInit {

  @Input() width: number;
  @Input() progress: number;
  @Input('explanation') progressExplanation: string;
  @Input() status: number = 1;

  constructor() { }

  ngOnInit() {
  }

}
