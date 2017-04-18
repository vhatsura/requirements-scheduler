import { Component, Input, Output, EventEmitter } from '@angular/core'
import { Test } from '../../models/index';

@Component({
  selector: 'test-list',
  template: `
  <div class="panel panel-default">
    <div class="panel-heading">
      <h3 class="panel-title">Tests</h3>
    </div>
    <ul class="nav nav-pills nav-stacked">
      <ng-template ngFor let-test [ngForOf]="tests" let-i="index">
        <li role="presentation" [class.active]="selected == test">
          <a (click)="selectedChange.next(test)">Test #{{test.testNumber}}. Is optimized on offline mode: {{test.isOptimized}}</a>
        </li>
      </ng-template>
    </ul>
  </div>
  `
})
export class TestListComponent {
  @Input() tests: Test[];
  @Input() selected: Test;
  @Output() selectedChange: EventEmitter<Test> = new EventEmitter();
}