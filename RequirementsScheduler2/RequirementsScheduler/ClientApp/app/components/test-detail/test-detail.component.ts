import {Component, Input, Output, EventEmitter} from '@angular/core';

import { Test } from '../../models/index';
//import {Customer} from './customerService';

@Component({
  selector: 'test-detail',
  template: `
  <div class="panel panel-default">
    <div *ngIf="!test" class="panel-heading">
      <h3 class="panel-title">Select a test</h3>
    </div>
    <div *ngIf="test" class="panel-heading">
      <h3 class="panel-title">{{test.testNumber}}</h3>
    </div>
    <ul class="list-group" *ngIf="test">
      <li class="list-group-item">
        <h4 class="list-group-item-heading">Test number</h4>
        <p class="list-group-item-text">{{test.testNumber}}</p>
      </li>
      <!--<li class="list-group-item">
        <h4 class="list-group-item-heading">Last Name</h4>
        <p class="list-group-item-text">{{customer.name.last}}</p>
      </li>-->
      <!--<li class="list-group-item">
        <h4 class="list-group-item-heading">First Name</h4>
        <p class="list-group-item-text">{{customer.email}}</p>
      </li>-->
  </ul>
  </div>
  `
})
export class TestDetailComponent {
  @Input() test: Test
  
}