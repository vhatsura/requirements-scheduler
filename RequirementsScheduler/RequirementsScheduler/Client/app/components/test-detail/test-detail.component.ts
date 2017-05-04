import { Component, Input, Output, EventEmitter } from '@angular/core';

import { Test } from '../../models/index';
// import {Customer} from './customerService';

@Component({
  selector: 'test-detail',
  templateUrl: './test-detail.component.html'
})
export class TestDetailComponent {
  @Input() test: Test;
  
}
