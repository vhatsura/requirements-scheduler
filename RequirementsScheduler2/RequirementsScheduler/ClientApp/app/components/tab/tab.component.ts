import { Component, Input } from '@angular/core';

@Component({
    selector: 'tab',
    styles: [`
    .pane{
      padding: 1em;
    }
  `],
    template: require('./tab.component.html'),
})
export class TabComponent {
    @Input('tabTitle') title: string;
    @Input() active = false;
}
