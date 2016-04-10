﻿<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Popup-Reject.aspx.cs" Inherits="Account_Popup_Rejected" %>
<div id="popup-reject" class="popup popup-reject">
	<h2 class="h2 heading">Reason</h2>
	<form class="form-reject" action="#" method="post" data-form="reject">
		<div class="form-line">
			<button class="button" data-reason="1">Bid is too <%=MyUtils.IsMale ? "high":"low" %></button>
		</div>
		<div class="form-line">
			<button class="button button-far-away" data-reason="2">Too far away</button>
		</div>
		<div class="form-line form-line-submit">
			<button class="button button-not-interested" data-reason="3">Not interested</button>
		</div>
	</form>
</div>
