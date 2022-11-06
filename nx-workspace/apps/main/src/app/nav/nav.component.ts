import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'gitlab-insight-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.scss'],
})
export class NavComponent implements OnInit {
  
	public isMenuCollapsed = true;

  constructor() {}

  ngOnInit(): void {}
}
