using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
namespace Kalkulator1
{
	public class printProtocol
	{
		private string header;
		private string footer;
		private string body;
		private string additionaContent;
		private string raport;
		private codeBar code;
		private bool isSigned;
		private string path;
		private System.Collections.Generic.Dictionary<string, string> errors;
		public printProtocol()
		{
			this.path = System.IO.Path.GetTempPath() + "KBW";
			this.header = "<!DOCTYPE HTML><html><head><meta charset='UTF-8'><title></title><link rel='stylesheet' type='text/css' href='" + System.IO.Path.GetDirectoryName(Application.StartupPath) + "\\tmp\\printTmp\\css\\styl.css'>";
			this.header = this.header + "<script type=\"text/javascript\" src=\"" + System.IO.Path.GetDirectoryName(Application.StartupPath) + "\\tmp\\printTmp\\jquery-1.11.1.min.js\"></script>";
			this.header += "</head><body>";
			this.footer = "</body></html>";
			this.body = "";
			this.additionaContent = "";
			this.code = new codeBar();
			this.isSigned = false;
			this.setErrorsTab();
		}
		public printProtocol(string controlSum, PictureBox p)
		{
			this.path = System.IO.Path.GetTempPath() + "KBW";
			this.header = "<!DOCTYPE HTML><html><head><meta charset='UTF-8'><title></title><link rel='stylesheet' type='text/css' href='" + System.IO.Path.GetDirectoryName(Application.StartupPath) + "\\tmp\\printTmp\\css\\styl.css'>";
			this.header = this.header + "<script type=\"text/javascript\" src=\"" + System.IO.Path.GetDirectoryName(Application.StartupPath) + "\\tmp\\printTmp\\jquery-1.11.1.min.js\"></script>";
			this.header += "</head><body>";
			this.body = "";
			this.additionaContent = "";
			this.code = new codeBar();
			this.code.generateCode(controlSum);
			this.code.draw(p);
			this.isSigned = true;
			this.setErrorsTab();
		}
		public string getProtocol(XmlDocument design, XmlDocument value, XmlDocument candidates)
		{
			bool comitteline = false;
			string response = "";
			string pathSave = "";
			string tableOfElection = "";
			XmlNode stepSave = value.SelectSingleNode("/save/step");
			if (stepSave != null && stepSave.InnerText == "0")
			{
				this.isSigned = true;
			}
			bool readHeader = false;
			string startTable = "<table>";
			string endTable = "</table>";
			try
			{
				XmlNode nodesList = design.SelectSingleNode("/protokol_info");
				foreach (XmlNode fields in nodesList)
				{
					if (fields.Name == "fields")
					{
						XmlNode typeFields = fields.Attributes.GetNamedItem("type");
						if (typeFields != null && typeFields.Value == "header")
						{
							string contentHeader = "";
							foreach (XmlNode box in fields)
							{
								bool border = false;
								if (box.Name == "box" && box.Attributes.GetNamedItem("design") != null)
								{
									if (box.Attributes.GetNamedItem("design").Value == "border")
									{
										border = true;
									}
								}
								if (border)
								{
									contentHeader += startTable;
								}
								foreach (XmlNode item in box)
								{
									if (border)
									{
										if (item.Name == "title")
										{
											XmlNode colspan = item.Attributes.GetNamedItem("colspan");
											if (colspan != null)
											{
												contentHeader = contentHeader + "<tr><td colspan=\"" + colspan.Value + "\" class=\"center\">";
											}
											else
											{
												contentHeader += "<tr><td>";
											}
											XmlNode bold = item.Attributes.GetNamedItem("bold");
											if (bold != null && bold.Value == "true")
											{
												contentHeader = contentHeader + "<b>" + item.InnerXml + "</b>";
											}
											else
											{
												contentHeader += item.InnerXml;
											}
											contentHeader += "</td></tr>";
										}
										if (item.Name == "row")
										{
											contentHeader += "<tr>";
											for (int i = 0; i < item.ChildNodes.Count; i++)
											{
												XmlNode name = item.ChildNodes[i].Attributes.GetNamedItem("name");
												XmlNode colspan = item.ChildNodes[i].Attributes.GetNamedItem("colspan");
												if (colspan != null)
												{
													contentHeader = contentHeader + "<td colspan=\"" + colspan.Value + "\">";
												}
												else
												{
													contentHeader += "<td>";
												}
												if (name != null)
												{
													contentHeader += name.Value;
												}
												contentHeader += "</td>";
												XmlNode colspan_save = item.ChildNodes[i].Attributes.GetNamedItem("colspan_save");
												XmlNode ischar = item.ChildNodes[i].Attributes.GetNamedItem("char");
												XmlNode saveCheck = item.ChildNodes[i].Attributes.GetNamedItem("saveCheck");
												XmlNode save_as = item.ChildNodes[i].Attributes.GetNamedItem("save_as");
												if (save_as != null)
												{
													string classIs = "";
													XmlNode iscode = value.SelectSingleNode("/save/hardWarning");
													XmlNode iscode2 = value.SelectSingleNode("/save/softError");
													XmlNode valueSaveAs = value.SelectSingleNode("/save/header/" + save_as.Value);
													if (valueSaveAs != null)
													{
														if (saveCheck != null && saveCheck.Value.ToLower() == "true")
														{
															if ((iscode != null && iscode.ChildNodes.Count != 0) || (iscode2 != null && iscode2.ChildNodes.Count != 0))
															{
																classIs = " class=\"warningCheck\" ";
															}
														}
														if (ischar != null && ischar.Value == "true" && colspan_save != null && colspan_save.Value != "" && System.Convert.ToInt32(colspan_save.Value) > 0 && valueSaveAs.InnerText.Length <= System.Convert.ToInt32(colspan_save.Value))
														{
															char[] sign = valueSaveAs.InnerText.ToCharArray();
															string contentChar = "";
															for (int j = 0; j < sign.Length; j++)
															{
																string text2 = contentChar;
																contentChar = string.Concat(new string[]
																{
																	text2,
																	"<td colspan=\"1\" ",
																	classIs,
																	">",
																	sign[j].ToString(),
																	"</td>"
																});
															}
															string pattern = " ";
															XmlNode fill = item.ChildNodes[i].Attributes.GetNamedItem("fill");
															if (fill != null && fill.Value != "")
															{
																pattern = fill.Value;
															}
															int empty = System.Convert.ToInt32(colspan_save.Value) - sign.Length;
															for (int j = 0; j < empty; j++)
															{
																contentChar = "<td colspan=\"1\">" + pattern + "</td>" + contentChar;
															}
															contentHeader += contentChar;
														}
														else
														{
															if (colspan_save != null)
															{
																string text2 = contentHeader;
																contentHeader = string.Concat(new string[]
																{
																	text2,
																	"<td colspan=\"",
																	colspan_save.Value,
																	"\"",
																	classIs,
																	">"
																});
															}
															else
															{
																contentHeader = contentHeader + "<td" + classIs + ">";
															}
															contentHeader += valueSaveAs.InnerText;
														}
													}
													else
													{
														if (colspan_save != null)
														{
															string text2 = contentHeader;
															contentHeader = string.Concat(new string[]
															{
																text2,
																"<td colspan=\"",
																colspan_save.Value,
																"\"",
																classIs,
																">"
															});
														}
														else
														{
															contentHeader = contentHeader + "<td" + classIs + ">";
														}
													}
												}
												else
												{
													if (colspan_save != null)
													{
														contentHeader = contentHeader + "<td colspan=\"" + colspan_save.Value + "\">";
													}
													else
													{
														contentHeader += "<td>";
													}
												}
												contentHeader += "</td>";
											}
											contentHeader += "</tr>";
										}
									}
									else
									{
										if (item.Name == "title")
										{
											XmlNode c = item.Attributes.GetNamedItem("class");
											string c2 = "";
											if (c != null && c.Value != "")
											{
												c2 = " class=\"" + c.Value + "\"";
											}
											XmlNode bold = item.Attributes.GetNamedItem("bold");
											if (bold != null && bold.Value == "true")
											{
												string text2 = contentHeader;
												contentHeader = string.Concat(new string[]
												{
													text2,
													"<h1",
													c2,
													"><b>",
													item.InnerXml,
													"</b></h1>"
												});
											}
											else
											{
												contentHeader = contentHeader + "<h1>" + item.InnerXml + "</h1>";
											}
										}
										if (item.Name == "row")
										{
											contentHeader += "<p>";
											for (int i = 0; i < item.ChildNodes.Count; i++)
											{
												XmlNode name = item.ChildNodes[i].Attributes.GetNamedItem("name");
												if (name != null)
												{
													contentHeader = contentHeader + name.Value + " ";
												}
												XmlNode save_as = item.ChildNodes[i].Attributes.GetNamedItem("save_as");
												if (save_as != null)
												{
													XmlNode valueSaveAs = value.SelectSingleNode("/save/header/" + save_as.Value);
													if (valueSaveAs != null)
													{
														contentHeader = contentHeader + valueSaveAs.InnerText + " ";
													}
												}
											}
											contentHeader += "</p>";
										}
									}
								}
								if (!readHeader)
								{
									this.raport += contentHeader;
									readHeader = true;
								}
								if (border)
								{
									contentHeader += endTable;
								}
							}
							this.body += contentHeader;
						}
						if (typeFields != null && typeFields.Value == "calculator")
						{
							string content = "";
							if (stepSave != null)
							{
								string step = stepSave.InnerText;
								if (step == "0" || step == "4")
								{
									pathSave = "/save/form";
								}
								if (step == "1" || step == "2" || step == "3")
								{
									pathSave = "/save/step1";
								}
							}
							bool predBorder = false;
							bool border = false;
							foreach (XmlNode item in fields)
							{
								XmlNode borderInfo = item.Attributes.GetNamedItem("design");
								predBorder = border;
								border = (borderInfo != null && borderInfo.Value == "border");
								if (!predBorder && border)
								{
									content += startTable;
								}
								if (predBorder && !border)
								{
									content += endTable;
								}
								if (item.Name == "title")
								{
									XmlNode bold = item.Attributes.GetNamedItem("bold");
									if (border)
									{
										XmlNode colspan = item.Attributes.GetNamedItem("colspan");
										if (colspan != null)
										{
											content = content + "<tr><td colspan=\"" + colspan.Value + "\" class=\"center\">";
										}
										else
										{
											content += "<tr><td>";
										}
										if (bold != null && bold.Value == "true")
										{
											content = content + "<b>" + item.InnerXml + "</b>";
										}
										else
										{
											content += item.InnerXml;
										}
										content += "</td><tr>";
									}
									else
									{
										if (bold != null && bold.Value == "true")
										{
											content = content + "<h1><b>" + item.InnerXml + "</b></h1>";
										}
										else
										{
											content = content + "<h1>" + item.InnerXml + "</h1>";
										}
									}
								}
								if (item.Name == "description")
								{
									XmlNode bold = item.Attributes.GetNamedItem("bold");
									if (border)
									{
										XmlNode colspan = item.Attributes.GetNamedItem("colspan");
										if (colspan != null)
										{
											content = content + "<tr><td colspan=\"" + colspan.Value + "\">";
										}
										else
										{
											content += "<tr><td>";
										}
										if (bold != null && bold.Value == "true")
										{
											content = content + "<b>" + item.InnerXml + "</b>";
										}
										else
										{
											content += item.InnerXml;
										}
										content += "</td><tr>";
									}
									else
									{
										if (bold != null && bold.Value == "true")
										{
											content = content + "<p><b>" + item.InnerXml + "</b></p>";
										}
										else
										{
											content = content + "<p>" + item.InnerXml + "</p>";
										}
									}
								}
								if (item.Name == "note")
								{
									XmlNode bold = item.Attributes.GetNamedItem("bold");
									if (border)
									{
										XmlNode colspan = item.Attributes.GetNamedItem("colspan");
										if (colspan != null)
										{
											content = content + "<tr><td colspan=\"" + colspan.Value + "\">";
										}
										else
										{
											content += "<tr><td>";
										}
										if (bold != null && bold.Value == "true")
										{
											content = content + "<b>Uwaga!</b><b>" + item.InnerXml + "</b>";
										}
										else
										{
											content += item.InnerXml;
										}
										content += "</td><tr>";
									}
									else
									{
										if (bold != null && bold.Value == "true")
										{
											content = content + "<table class=\"noborder\" ><tr><td colspan=\"2\"><b>Uwaga!</b></td><td colspan=\"22\"><b>" + item.InnerXml + "</b></td></tr></table>";
										}
										else
										{
											content = content + "<table class=\"noborder\"><tr><td colspan=\"2\"><b>Uwaga!</b></td><td colspan=\"22\">" + item.InnerXml + "</td></tr></table>";
										}
									}
								}
								if (item.Name == "field")
								{
									string contentField = "";
									XmlNode lp = item.Attributes.GetNamedItem("lp");
									if (border)
									{
										contentField += "<tr>";
										XmlNode colspan = item.Attributes.GetNamedItem("colspan");
										XmlNode colspan_save = item.Attributes.GetNamedItem("colspan_save");
										XmlNode colspan_nr = item.Attributes.GetNamedItem("colspan_nr");
										if (lp != null && lp.Value != "" && colspan_nr != null && colspan_nr.Value != "" && System.Convert.ToInt32(colspan_nr.Value) > 0)
										{
											string text2 = contentField;
											contentField = string.Concat(new string[]
											{
												text2,
												"<td colspan =\"",
												colspan_nr.Value,
												"\" class=\"lp\">",
												lp.Value,
												". </td>"
											});
										}
										for (int j = 0; j < item.ChildNodes.Count; j++)
										{
											string text = item.ChildNodes[j].InnerXml.Replace(System.Environment.NewLine, "<br>");
											if (item.ChildNodes[j].Name == "name" && colspan != null && colspan.Value != "" && System.Convert.ToInt32(colspan.Value) > 0)
											{
												string text2 = contentField;
												contentField = string.Concat(new string[]
												{
													text2,
													"<td colspan =\"",
													colspan.Value,
													"\">",
													text,
													"</td>"
												});
											}
											if (item.ChildNodes[j].Name == "save_as")
											{
												XmlNode ischar = item.ChildNodes[j].Attributes.GetNamedItem("char");
												XmlNode valueSaveAs = value.SelectSingleNode(pathSave + "/" + item.ChildNodes[j].InnerText);
												if (valueSaveAs != null)
												{
													if (ischar != null && ischar.Value == "true" && colspan_save != null && colspan_save.Value != "" && System.Convert.ToInt32(colspan_save.Value) > 0 && valueSaveAs.InnerText.Length <= System.Convert.ToInt32(colspan_save.Value))
													{
														char[] sign = valueSaveAs.InnerText.ToCharArray();
														string contentChar = "";
														for (int k = 0; k < sign.Length; k++)
														{
															contentChar = contentChar + "<td colspan=\"1\">" + sign[k].ToString() + "</td>";
														}
														string pattern = " ";
														XmlNode fill = item.ChildNodes[j].Attributes.GetNamedItem("fill");
														if (fill != null && fill.Value != "")
														{
															pattern = fill.Value;
														}
														int empty = System.Convert.ToInt32(colspan_save.Value) - sign.Length;
														for (int k = 0; k < empty; k++)
														{
															contentChar = "<td colspan=\"1\">" + pattern + "</td>" + contentChar;
														}
														contentField += contentChar;
													}
													else
													{
														if (colspan_save != null)
														{
															contentField = contentField + "<td colspan=\"" + colspan_save.Value + "\">";
														}
														else
														{
															contentField += "<td>";
														}
														contentField += valueSaveAs.InnerText;
														contentField += "</td>";
													}
												}
												else
												{
													if (colspan_save != null)
													{
														contentField = contentField + "<td colspan=\"" + colspan_save.Value + "\"> </td>";
													}
													else
													{
														contentField += "<td> </td>";
													}
												}
											}
										}
										contentField += "</tr>";
									}
									else
									{
										contentField += "<p>";
										if (lp != null && lp.Value != "")
										{
											contentField = contentField + lp.Value + " ";
										}
										for (int j = 0; j < item.ChildNodes.Count; j++)
										{
											if (item.ChildNodes[j].Name == "name")
											{
												contentField = contentField + item.ChildNodes[j].InnerXml + " ";
											}
											if (item.ChildNodes[j].Name == "save_as")
											{
												XmlNode valueSaveAs = value.SelectSingleNode(pathSave + "/" + item.ChildNodes[j].InnerText);
												if (valueSaveAs != null)
												{
													contentField += valueSaveAs.Value;
												}
											}
										}
										contentField += "</p>";
									}
									content += contentField;
								}
							}
							if (!predBorder && border)
							{
								content += startTable;
							}
							if (predBorder && !border)
							{
								content += endTable;
							}
							this.body += content;
						}
						if (typeFields != null && typeFields.Value == "additional-calculator")
						{
							string content = "";
							if (stepSave != null)
							{
								string step = stepSave.InnerText;
								if (step == "0" || step == "4")
								{
									pathSave = "/save/form";
								}
								if (step == "3")
								{
									pathSave = "/save/step3";
								}
							}
							bool predBorder = false;
							bool border = false;
							foreach (XmlNode item in fields)
							{
								XmlNode borderInfo = item.Attributes.GetNamedItem("design");
								predBorder = border;
								border = (borderInfo != null && borderInfo.Value == "border");
								if (!predBorder && border)
								{
									content += startTable;
								}
								if (predBorder && !border)
								{
									content += endTable;
								}
								if (item.Name == "title")
								{
									if (stepSave != null && stepSave.InnerText == "0")
									{
										content = "<div class=\"break\">&#8194</div>";
									}
									XmlNode bold = item.Attributes.GetNamedItem("bold");
									if (border)
									{
										XmlNode colspan = item.Attributes.GetNamedItem("colspan");
										if (colspan != null)
										{
											content = content + "<tr><td colspan=\"" + colspan.Value + "\" class=\"center\">";
										}
										else
										{
											content += "<tr><td>";
										}
										if (bold != null && bold.Value == "true")
										{
											content = content + "<b>" + item.InnerXml + "</b>";
										}
										else
										{
											content += item.InnerXml;
										}
										content += "</td><tr>";
									}
									else
									{
										if (bold != null && bold.Value == "true")
										{
											content = content + "<h1><b>" + item.InnerXml + "</b></h1>";
										}
										else
										{
											content = content + "<h1>" + item.InnerXml + "</h1>";
										}
									}
								}
								if (item.Name == "field")
								{
									XmlNode kandydaci = item.Attributes.GetNamedItem("data");
									if (kandydaci != null && kandydaci.Value == "kandydaci")
									{
										if (stepSave != null && stepSave.InnerText == "0")
										{
											content = "<div class=\"break\"> &#8194 </div>";
										}
										else
										{
											content = "<div id=\"breakTab2\"> &#8194 </div>";
										}
										XmlNode candidatesRoot = candidates.SelectSingleNode("/listy");
										if (item.Name == "field")
										{
											XmlNode lp = item.Attributes.GetNamedItem("lp");
											foreach (XmlNode itemChild in item)
											{
												if (itemChild.Name == "name")
												{
													XmlNode bold = itemChild.Attributes.GetNamedItem("bold");
													if (border)
													{
														content += "<tr>";
														XmlNode colspan_nr = itemChild.Attributes.GetNamedItem("colspan_nr");
														if (lp != null && lp.Value != "" && colspan_nr != null && colspan_nr.Value != "" && System.Convert.ToInt32(colspan_nr.Value) > 0)
														{
															string text2 = content;
															content = string.Concat(new string[]
															{
																text2,
																"<td colspan =\"",
																colspan_nr.Value,
																"\">",
																lp.Value,
																". </td>"
															});
														}
														XmlNode colspan = itemChild.Attributes.GetNamedItem("colspan");
														if (colspan != null)
														{
															content = content + "<td colspan=\"" + colspan.Value + "\">";
														}
														else
														{
															content += "<td>";
														}
														if (bold != null && bold.Value == "true")
														{
															content = content + "<b>" + itemChild.InnerXml + "</b>";
														}
														else
														{
															content += itemChild.InnerXml;
														}
														content += "</td><tr>";
													}
													else
													{
														content += "<p>";
														if (lp != null && lp.Value != "")
														{
															content = content + lp.Value + ". ";
														}
														if (bold != null && bold.Value == "true")
														{
															content = content + "<b>" + itemChild.InnerXml + "</b></p>";
														}
														else
														{
															content = content + itemChild.InnerXml + "</p>";
														}
													}
												}
												if (itemChild.Name == "note")
												{
													borderInfo = item.Attributes.GetNamedItem("design");
													border = (borderInfo != null && borderInfo.Value == "border");
													if (!predBorder && border)
													{
														content += startTable;
													}
													if (predBorder && !border)
													{
														content += endTable;
													}
													XmlNode bold = itemChild.Attributes.GetNamedItem("bold");
													if (border)
													{
														XmlNode colspan = itemChild.Attributes.GetNamedItem("colspan");
														if (colspan != null)
														{
															content = content + "<tr><td colspan=\"" + colspan.Value + "\">";
														}
														else
														{
															content += "<tr><td>";
														}
														if (bold != null && bold.Value == "true")
														{
															content = content + "<b>Uwaga!</b><b>" + itemChild.InnerXml + "</b>";
														}
														else
														{
															content += itemChild.InnerXml;
														}
														content += "</td><tr>";
													}
													else
													{
														if (bold != null && bold.Value == "true")
														{
															content = content + "<table class=\"noborder\" ><tr><td colspan=\"2\"><b>Uwaga!</b></td><td colspan=\"22\"><b>" + itemChild.InnerXml + "</b></td></tr></table>";
														}
														else
														{
															content = content + "<p></p><table class=\"noborder\"><tr><td colspan=\"2\"><b>Uwaga!</b></td><td colspan=\"22\">" + itemChild.InnerXml + "</td></tr></table>";
														}
													}
												}
												if (itemChild.Name == "patternrows")
												{
													string contentCandidate = "";
													borderInfo = itemChild.Attributes.GetNamedItem("design");
													predBorder = border;
													border = (borderInfo != null && borderInfo.Value == "border");
													if (!predBorder && border)
													{
														contentCandidate += startTable;
													}
													if (predBorder && !border)
													{
														contentCandidate += endTable;
													}
													XmlNode colspan = itemChild.Attributes.GetNamedItem("colspan");
													XmlNode colspan_save = itemChild.Attributes.GetNamedItem("colspan_save");
													XmlNode colspan_nr = itemChild.Attributes.GetNamedItem("colspan_nr");
													XmlNode lp2 = itemChild.Attributes.GetNamedItem("lp");
													System.Collections.Generic.List<Field> patternField = new System.Collections.Generic.List<Field>();
													patternField = this.readPatternCandidate(itemChild, patternField);
													int save_as_candidate = 1;
													foreach (XmlNode lista in candidatesRoot)
													{
														if (border)
														{
															if (save_as_candidate % 17 == 0)
															{
																contentCandidate += "</table><div class=\"break\"> &#8194 </div><table><tr>";
															}
															else
															{
																contentCandidate += "<tr>";
															}
														}
														for (int i = 0; i < patternField.Count; i++)
														{
															string name2 = "";
															string status = this.getStatusBasePatternCandidate(patternField[i], lista);
															if (status == "A" || status == "R")
															{
																string imie = this.getImie1BasePatternCandidate(patternField[i], lista);
																string imie2 = this.getImie2BasePatternCandidate(patternField[i], lista);
																string nazwisko = this.getNazwiskoBasePatternCandidate(patternField[i], lista);
																string komitet = this.getKomitetBasePatternCandidate(patternField[i], lista);
																string name3 = this.getName2BasePatternCandidate(patternField[i], lista);
																if (patternField[i].getName1() != "")
																{
																	name2 = patternField[i].getName1() + " ";
																}
																string text = string.Concat(new string[]
																{
																	"<b>",
																	name2,
																	nazwisko,
																	imie,
																	imie2,
																	"</b><br>",
																	name3,
																	komitet
																});
																if (border)
																{
																	if (patternField[i].getDataType() == "text" && patternField[i].getSave() == "")
																	{
																		if (lp2 != null && lp2.Value == "auto")
																		{
																			if (colspan_nr != null && colspan_nr.Value != "" && System.Convert.ToInt32(colspan_nr.Value) > 0)
																			{
																				string text2 = contentCandidate;
																				contentCandidate = string.Concat(new string[]
																				{
																					text2,
																					"<td colspan=\"",
																					colspan_nr.Value,
																					"\" class=\"lp\">",
																					save_as_candidate.ToString(),
																					". </td>"
																				});
																			}
																			else
																			{
																				contentCandidate = contentCandidate + "<td>" + save_as_candidate.ToString() + "</td>";
																			}
																		}
																		if (colspan != null && colspan.Value != "" && System.Convert.ToInt32(colspan.Value) > 0)
																		{
																			string text2 = contentCandidate;
																			contentCandidate = string.Concat(new string[]
																			{
																				text2,
																				"<td colspan=\"",
																				colspan.Value,
																				"\">",
																				text,
																				"</td>"
																			});
																		}
																		else
																		{
																			contentCandidate = contentCandidate + "<td>" + text + "</td>";
																		}
																	}
																	if (patternField[i].getDataType() == "number" && patternField[i].getSave() != "")
																	{
																		string nameSaveAs = patternField[i].getSave().Replace("X", save_as_candidate.ToString());
																		XmlNode valueSaveAs = value.SelectSingleNode(pathSave + "/" + nameSaveAs);
																		if (valueSaveAs != null)
																		{
																			if (status != "R")
																			{
																				if (patternField[i].isChar() && colspan_save != null && colspan_save.Value != "" && System.Convert.ToInt32(colspan_save.Value) > 0 && valueSaveAs.InnerText.Length <= System.Convert.ToInt32(colspan_save.Value))
																				{
																					char[] sign = valueSaveAs.InnerText.ToCharArray();
																					string contentChar = "";
																					for (int k = 0; k < sign.Length; k++)
																					{
																						contentChar = contentChar + "<td colspan=\"1\">" + sign[k].ToString() + "</td>";
																					}
																					string pattern = " ";
																					if (patternField[i].getFill() != "")
																					{
																						pattern = patternField[i].getFill();
																					}
																					int empty = System.Convert.ToInt32(colspan_save.Value) - sign.Length;
																					for (int k = 0; k < empty; k++)
																					{
																						contentChar = "<td colspan=\"1\">" + pattern + "</td>" + contentChar;
																					}
																					contentCandidate += contentChar;
																				}
																				else
																				{
																					if (colspan_save != null)
																					{
																						contentCandidate = contentCandidate + "<td colspan=\"" + colspan_save.Value + "\">";
																					}
																					else
																					{
																						contentCandidate += "<td>";
																					}
																					contentCandidate += valueSaveAs.InnerText;
																					contentCandidate += "</td>";
																				}
																			}
																			else
																			{
																				if (colspan_save != null)
																				{
																					contentCandidate = contentCandidate + "<td colspan=\"" + colspan_save.Value + "\">";
																				}
																				else
																				{
																					contentCandidate += "<td>";
																				}
																				contentCandidate += "Skre??lony";
																				contentCandidate += "</td>";
																			}
																		}
																		else
																		{
																			if (colspan_save != null)
																			{
																				contentCandidate = contentCandidate + "<td colspan=\"" + colspan_save.Value + "\">";
																			}
																			else
																			{
																				contentCandidate += "<td>";
																			}
																			if (status == "R")
																			{
																				contentCandidate += "Skre??lony";
																			}
																			else
																			{
																				contentCandidate += " ";
																			}
																			contentCandidate += "</td>";
																		}
																		contentCandidate += "</tr>";
																	}
																}
																else
																{
																	contentCandidate += "<p>";
																	if (patternField[i].getDataType() == "text" && patternField[i].getSave() == "")
																	{
																		if (lp2 != null && lp2.Value == "auto")
																		{
																			contentCandidate = contentCandidate + save_as_candidate.ToString() + " ";
																		}
																		contentCandidate = contentCandidate + text + " ";
																	}
																	if (patternField[i].getDataType() == "number" && patternField[i].getSave() != "")
																	{
																		string nameSaveAs = patternField[i].getSave().Replace("X", save_as_candidate.ToString());
																		XmlNode valueSaveAs = value.SelectSingleNode(pathSave + "/" + nameSaveAs);
																		if (valueSaveAs != null)
																		{
																			if (status != "R")
																			{
																				contentCandidate += valueSaveAs.Value;
																			}
																			else
																			{
																				contentCandidate += "Skre??lony";
																			}
																		}
																	}
																	contentCandidate += "</p>";
																}
															}
														}
														save_as_candidate++;
													}
													if (border)
													{
														contentCandidate += "</tr>";
													}
													content += contentCandidate;
												}
												if (itemChild.Name == "patternrow")
												{
													string contentCandidate = "";
													borderInfo = itemChild.Attributes.GetNamedItem("design");
													predBorder = border;
													border = (borderInfo != null && borderInfo.Value == "border");
													if (!predBorder && border)
													{
														contentCandidate += startTable;
													}
													if (predBorder && !border)
													{
														contentCandidate += endTable;
													}
													XmlNode colspan = itemChild.Attributes.GetNamedItem("colspan");
													XmlNode colspan_save = itemChild.Attributes.GetNamedItem("colspan_save");
													XmlNode colspan_nr = itemChild.Attributes.GetNamedItem("colspan_nr");
													XmlNode lp2 = itemChild.Attributes.GetNamedItem("lp");
													System.Collections.Generic.List<Field> patternField = new System.Collections.Generic.List<Field>();
													patternField = this.readPatternCandidate(itemChild, patternField);
													XmlNode borderField = itemChild.FirstChild.Attributes.GetNamedItem("design");
													XmlNode r = itemChild.FirstChild.Attributes.GetNamedItem("class");
													string r2 = "";
													if (r != null && r.Value != "" && borderField == null)
													{
														r2 = " class=\"" + r.Value + "\"";
													}
													if (r != null && r.Value != "" && borderField != null && borderField.Value != "")
													{
														r2 = string.Concat(new string[]
														{
															" class=\"",
															r.Value,
															" ",
															borderField.Value,
															"\""
														});
													}
													if (r == null && borderField != null && borderField.Value != "")
													{
														r2 = " class=\"" + borderField.Value + "\"";
													}
													int save_as_candidate = 1;
													if (border)
													{
														contentCandidate += "<tr>";
													}
													for (int i = 0; i < patternField.Count; i++)
													{
														string text = patternField[i].getName1();
														if (border)
														{
															if (lp2 != null && lp2.Value == "auto")
															{
																if (colspan_nr != null && colspan_nr.Value != "" && System.Convert.ToInt32(colspan_nr.Value) > 0)
																{
																	string text2 = contentCandidate;
																	contentCandidate = string.Concat(new string[]
																	{
																		text2,
																		"<td colspan=\"",
																		colspan_nr.Value,
																		"\">",
																		save_as_candidate.ToString(),
																		"</td>"
																	});
																}
																else
																{
																	contentCandidate = contentCandidate + "<td>" + save_as_candidate.ToString() + "</td>";
																}
															}
															if (colspan != null && colspan.Value != "" && System.Convert.ToInt32(colspan.Value) > 0)
															{
																string text2 = contentCandidate;
																contentCandidate = string.Concat(new string[]
																{
																	text2,
																	"<td colspan=\"",
																	colspan.Value,
																	"\"",
																	r2,
																	">",
																	text,
																	"</td>"
																});
															}
															else
															{
																string text2 = contentCandidate;
																contentCandidate = string.Concat(new string[]
																{
																	text2,
																	"<td",
																	r2,
																	">",
																	text,
																	"</td>"
																});
															}
															string nameSaveAs = patternField[i].getSave();
															XmlNode valueSaveAs = value.SelectSingleNode(pathSave + "/" + nameSaveAs);
															if (valueSaveAs != null)
															{
																if (patternField[i].isChar() && colspan_save != null && colspan_save.Value != "" && System.Convert.ToInt32(colspan_save.Value) > 0 && valueSaveAs.InnerText.Length <= System.Convert.ToInt32(colspan_save.Value))
																{
																	char[] sign = valueSaveAs.InnerText.ToCharArray();
																	string contentChar = "";
																	for (int k = 0; k < sign.Length; k++)
																	{
																		contentChar = contentChar + "<td colspan=\"1\">" + sign[k].ToString() + "</td>";
																	}
																	string pattern = " ";
																	if (patternField[i].getFill() != "")
																	{
																		pattern = patternField[i].getFill();
																	}
																	int empty = System.Convert.ToInt32(colspan_save.Value) - sign.Length;
																	for (int k = 0; k < empty; k++)
																	{
																		contentChar = "<td colspan=\"1\">" + pattern + "</td>" + contentChar;
																	}
																	contentCandidate += contentChar;
																}
																else
																{
																	if (colspan_save != null)
																	{
																		contentCandidate = contentCandidate + "<td colspan=\"" + colspan_save.Value + "\">";
																	}
																	else
																	{
																		contentCandidate += "<td>";
																	}
																	contentCandidate += valueSaveAs.InnerText;
																	contentCandidate += "</td>";
																}
															}
															else
															{
																if (colspan_save != null)
																{
																	contentCandidate = contentCandidate + "<td colspan=\"" + colspan_save.Value + "\">";
																}
																else
																{
																	contentCandidate += "<td>";
																}
																contentCandidate += " ";
																contentCandidate += "</td>";
															}
														}
														else
														{
															contentCandidate += "<p>";
															if (patternField[i].getDataType() == "text" && patternField[i].getSave() == "")
															{
																if (lp2 != null && lp2.Value == "auto")
																{
																	contentCandidate = contentCandidate + save_as_candidate.ToString() + " ";
																}
																contentCandidate = contentCandidate + text + " ";
															}
															if (patternField[i].getDataType() == "number" && patternField[i].getSave() != "")
															{
																string nameSaveAs = patternField[i].getSave().Replace("X", save_as_candidate.ToString());
																XmlNode valueSaveAs = value.SelectSingleNode(pathSave + "/" + nameSaveAs);
																if (valueSaveAs != null)
																{
																	contentCandidate += valueSaveAs.Value;
																}
															}
															contentCandidate += "</p>";
														}
														save_as_candidate++;
													}
													if (border)
													{
														contentCandidate += "</tr>";
													}
													content += contentCandidate;
												}
												if (itemChild.Name == "patternlist")
												{
													XmlNode borderList = itemChild.Attributes.GetNamedItem("design");
													bool borderL = false;
													if (borderList != null && borderList.Value == "border")
													{
														content += startTable;
														borderL = true;
													}
													int nrListy = 1;
													foreach (XmlNode lista in candidatesRoot)
													{
														foreach (XmlNode paternNode in itemChild)
														{
															if (paternNode.Name == "title")
															{
																XmlNode bold = paternNode.Attributes.GetNamedItem("bold");
																XmlNode nr = paternNode.Attributes.GetNamedItem("nr");
																string text = paternNode.InnerXml;
																if (nr != null)
																{
																	text = string.Concat(new string[]
																	{
																		nr.Value,
																		" ",
																		nrListy.ToString(),
																		" ",
																		text
																	});
																}
																if (borderL)
																{
																	XmlNode colspan = paternNode.Attributes.GetNamedItem("colspan");
																	if (colspan != null)
																	{
																		content = content + "<tr><td colspan=\"" + colspan.Value + "\">";
																	}
																	else
																	{
																		content += "<tr><td>";
																	}
																	if (bold != null && bold.Value == "true")
																	{
																		content = content + "<b>" + text + "</b>";
																	}
																	else
																	{
																		content += text;
																	}
																	content += "</td></tr>";
																}
																else
																{
																	if (bold != null && bold.Value == "true")
																	{
																		content = content + "<h1><b>" + text + "</b></h1>";
																	}
																	else
																	{
																		content = content + "<h1>" + text + "</h1>";
																	}
																}
															}
															if (paternNode.Name == "patternrow")
															{
																XmlNode colspan = paternNode.Attributes.GetNamedItem("colspan");
																XmlNode colspan_save = paternNode.Attributes.GetNamedItem("colspan_save");
																System.Collections.Generic.List<Field> patternField = new System.Collections.Generic.List<Field>();
																patternField = this.readPatternCandidate(paternNode, patternField);
																XmlNode r = paternNode.FirstChild.Attributes.GetNamedItem("class");
																string r2 = "";
																if (r != null && r.Value != "")
																{
																	r2 = " class=\"" + r.Value + "\"";
																}
																for (int i = 0; i < patternField.Count; i++)
																{
																	if (!patternField[i].needImportData())
																	{
																		if (borderL)
																		{
																			content += "<tr>";
																			if (colspan != null && colspan.Value != "" && System.Convert.ToInt32(colspan.Value) > 0)
																			{
																				string text2 = content;
																				content = string.Concat(new string[]
																				{
																					text2,
																					"<td colspan=\"",
																					colspan.Value,
																					"\"",
																					r2,
																					">",
																					patternField[i].getName1(),
																					"</td>"
																				});
																			}
																			else
																			{
																				string text2 = content;
																				content = string.Concat(new string[]
																				{
																					text2,
																					"<td",
																					r2,
																					">",
																					patternField[i].getName1(),
																					"</td>"
																				});
																			}
																			string nameSaveAs = patternField[i].getSave().Replace("Y", nrListy.ToString());
																			XmlNode valueSaveAs = value.SelectSingleNode(pathSave + "/" + nameSaveAs);
																			if (valueSaveAs != null)
																			{
																				if (patternField[i].isChar() && colspan_save != null && colspan_save.Value != "" && System.Convert.ToInt32(colspan_save.Value) > 0 && valueSaveAs.InnerText.Length <= System.Convert.ToInt32(colspan_save.Value))
																				{
																					char[] sign = valueSaveAs.InnerText.ToCharArray();
																					string contentChar = "";
																					for (int k = 0; k < sign.Length; k++)
																					{
																						contentChar = contentChar + "<td colspan=\"1\">" + sign[k].ToString() + "</td>";
																					}
																					string pattern = " ";
																					if (patternField[i].getFill() != "")
																					{
																						pattern = patternField[i].getFill();
																					}
																					int empty = System.Convert.ToInt32(colspan_save.Value) - sign.Length;
																					for (int k = 0; k < empty; k++)
																					{
																						contentChar = "<td colspan=\"1\">" + pattern + "</td>" + contentChar;
																					}
																					content += contentChar;
																				}
																				else
																				{
																					if (colspan_save != null)
																					{
																						content = content + "<td colspan=\"" + colspan_save.Value + "\">";
																					}
																					else
																					{
																						content += "<td>";
																					}
																					content += valueSaveAs.InnerText;
																					content += "</td>";
																				}
																			}
																			else
																			{
																				if (colspan_save != null)
																				{
																					content = content + "<td colspan=\"" + colspan_save.Value + "\">";
																				}
																				else
																				{
																					content += "<td>";
																				}
																				content += " ";
																				content += "</td>";
																			}
																			content += "</tr>";
																		}
																		else
																		{
																			content += "<p>";
																			content += patternField[i].getName1();
																			string nameSaveAs = patternField[i].getSave().Replace("Y", nrListy.ToString());
																			XmlNode valueSaveAs = value.SelectSingleNode(pathSave + "/" + nameSaveAs);
																			if (valueSaveAs != null)
																			{
																				content = content + " " + valueSaveAs.InnerText;
																			}
																			content += "</p>";
																		}
																	}
																}
															}
															if (paternNode.Name == "patternrows")
															{
																XmlNode colspan = paternNode.Attributes.GetNamedItem("colspan");
																XmlNode colspan_save = paternNode.Attributes.GetNamedItem("colspan_save");
																System.Collections.Generic.List<Field> patternField = new System.Collections.Generic.List<Field>();
																XmlNode colspan_nr = paternNode.Attributes.GetNamedItem("colspan_nr");
																XmlNode lp2 = paternNode.Attributes.GetNamedItem("lp");
																patternField = this.readPatternCandidate(paternNode, patternField);
																int save_as_candidate = 1;
																for (int l = 0; l < lista.ChildNodes.Count; l++)
																{
																	if (borderL)
																	{
																		content += "<tr>";
																	}
																	for (int i = 0; i < patternField.Count; i++)
																	{
																		string name2 = "";
																		string status = this.getStatusBasePatternCandidate(patternField[i], lista);
																		if (status == "A" || status == "R")
																		{
																			string imie = this.getImie1BasePatternCandidate(patternField[i], lista);
																			string imie2 = this.getImie2BasePatternCandidate(patternField[i], lista);
																			string nazwisko = this.getNazwiskoBasePatternCandidate(patternField[i], lista);
																			string komitet = this.getKomitetBasePatternCandidate(patternField[i], lista);
																			string name3 = this.getName2BasePatternCandidate(patternField[i], lista);
																			if (patternField[i].getName1() != "")
																			{
																				name2 = patternField[i].getName1() + " ";
																			}
																			string text = string.Concat(new string[]
																			{
																				name2,
																				nazwisko,
																				imie,
																				imie2,
																				name3,
																				komitet
																			});
																			if (borderL)
																			{
																				if (patternField[i].getDataType() == "text" && patternField[i].getSave() == "")
																				{
																					if (lp2 != null && lp2.Value == "auto")
																					{
																						if (colspan_nr != null && colspan_nr.Value != "" && System.Convert.ToInt32(colspan_nr.Value) > 0)
																						{
																							string text2 = content;
																							content = string.Concat(new string[]
																							{
																								text2,
																								"<td colspan=\"",
																								colspan_nr.Value,
																								"\" class=\"lp\">",
																								save_as_candidate.ToString(),
																								"</td>"
																							});
																						}
																						else
																						{
																							content = content + "<td>" + save_as_candidate.ToString() + "</td>";
																						}
																					}
																					if (colspan != null && colspan.Value != "" && System.Convert.ToInt32(colspan.Value) > 0)
																					{
																						string text2 = content;
																						content = string.Concat(new string[]
																						{
																							text2,
																							"<td colspan=\"",
																							colspan.Value,
																							"\">",
																							text,
																							"</td>"
																						});
																					}
																					else
																					{
																						content = content + "<td>" + text + "</td>";
																					}
																				}
																				if (patternField[i].getDataType() == "number" && patternField[i].getSave() != "")
																				{
																					string nameSaveAs = patternField[i].getSave().Replace("X", save_as_candidate.ToString());
																					nameSaveAs = nameSaveAs.Replace("Y", nrListy.ToString());
																					XmlNode valueSaveAs = value.SelectSingleNode(pathSave + "/" + nameSaveAs);
																					if (valueSaveAs != null)
																					{
																						if (status != "R")
																						{
																							if (patternField[i].isChar() && colspan_save != null && colspan_save.Value != "" && System.Convert.ToInt32(colspan_save.Value) > 0 && valueSaveAs.InnerText.Length <= System.Convert.ToInt32(colspan_save.Value))
																							{
																								char[] sign = valueSaveAs.InnerText.ToCharArray();
																								string contentChar = "";
																								for (int k = 0; k < sign.Length; k++)
																								{
																									contentChar = contentChar + "<td colspan=\"1\">" + sign[k].ToString() + "</td>";
																								}
																								string pattern = " ";
																								if (patternField[i].getFill() != "")
																								{
																									pattern = patternField[i].getFill();
																								}
																								int empty = System.Convert.ToInt32(colspan_save.Value) - sign.Length;
																								for (int k = 0; k < empty; k++)
																								{
																									contentChar = "<td colspan=\"1\">" + pattern + "</td>" + contentChar;
																								}
																								content += contentChar;
																							}
																							else
																							{
																								if (colspan_save != null)
																								{
																									content = content + "<td colspan=\"" + colspan_save.Value + "\">";
																								}
																								else
																								{
																									content += "<td>";
																								}
																								content += valueSaveAs.InnerText;
																								content += "</td>";
																							}
																							content += "</tr>";
																						}
																						else
																						{
																							if (colspan_save != null)
																							{
																								content = content + "<td colspan=\"" + colspan_save.Value + "\">";
																							}
																							else
																							{
																								content += "<td>";
																							}
																							content += "</td></tr>";
																						}
																					}
																					else
																					{
																						if (colspan_save != null)
																						{
																							content = content + "<td colspan=\"" + colspan_save.Value + "\">";
																						}
																						else
																						{
																							content += "<td>";
																						}
																						if (status == "R")
																						{
																							content += "Skre??lony";
																						}
																						else
																						{
																							content += " ";
																						}
																						content += "</td>";
																						content += "</tr>";
																					}
																				}
																			}
																			else
																			{
																				content += "<p>";
																				if (patternField[i].getDataType() == "text" && patternField[i].getSave() == "")
																				{
																					if (lp2 != null && lp2.Value == "auto")
																					{
																						content = content + save_as_candidate.ToString() + " ";
																					}
																					content = content + text + " ";
																				}
																				if (patternField[i].getDataType() == "number" && patternField[i].getSave() != "")
																				{
																					string nameSaveAs = patternField[i].getSave().Replace("X", save_as_candidate.ToString());
																					XmlNode valueSaveAs = value.SelectSingleNode(pathSave + "/" + nameSaveAs);
																					if (valueSaveAs != null)
																					{
																						content += valueSaveAs.Value;
																						if (status != "R")
																						{
																							content += valueSaveAs.Value;
																						}
																						else
																						{
																							content += "Skre??lony";
																						}
																					}
																				}
																				content += "</p>";
																			}
																		}
																	}
																	save_as_candidate++;
																}
															}
														}
														if (borderL)
														{
															content += endTable;
														}
														if (candidatesRoot.ChildNodes.Count > nrListy)
														{
															content = content + "<p></p>" + startTable;
														}
														nrListy++;
													}
												}
											}
										}
									}
									else
									{
										if (item.Name == "field")
										{
											string contentField = "";
											XmlNode lp = item.Attributes.GetNamedItem("lp");
											XmlNode przypis = item.Attributes.GetNamedItem("przypis");
											if (border)
											{
												contentField += "<tr>";
												XmlNode colspan = item.Attributes.GetNamedItem("colspan");
												XmlNode colspan_save = item.Attributes.GetNamedItem("colspan_save");
												XmlNode colspan_nr = item.Attributes.GetNamedItem("colspan_nr");
												if (lp != null && lp.Value != "" && colspan_nr != null && colspan_nr.Value != "" && System.Convert.ToInt32(colspan_nr.Value) > 0)
												{
													string text2 = contentField;
													contentField = string.Concat(new string[]
													{
														text2,
														"<td colspan =\"",
														colspan_nr.Value,
														"\" class=\"lp\">",
														lp.Value,
														"</td>"
													});
												}
												for (int j = 0; j < item.ChildNodes.Count; j++)
												{
													if (item.ChildNodes[j].Name == "name" && colspan != null && colspan.Value != "" && System.Convert.ToInt32(colspan.Value) > 0)
													{
														string p = "";
														if (przypis != null && przypis.Value != "")
														{
															p = "<sup>" + przypis.Value + "</sup> ";
														}
														string text2 = contentField;
														contentField = string.Concat(new string[]
														{
															text2,
															"<td colspan =\"",
															p,
															colspan.Value,
															"\">",
															item.ChildNodes[j].InnerXml,
															"</td>"
														});
													}
													if (item.ChildNodes[j].Name == "save_as")
													{
														XmlNode ischar = item.ChildNodes[j].Attributes.GetNamedItem("char");
														XmlNode valueSaveAs = value.SelectSingleNode(pathSave + "/" + item.ChildNodes[j].InnerText);
														if (valueSaveAs != null)
														{
															if (ischar != null && ischar.Value == "true" && colspan_save != null && colspan_save.Value != "" && System.Convert.ToInt32(colspan_save.Value) > 0 && valueSaveAs.InnerText.Length <= System.Convert.ToInt32(colspan_save.Value))
															{
																char[] sign = valueSaveAs.InnerText.ToCharArray();
																string contentChar = "";
																for (int k = 0; k < sign.Length; k++)
																{
																	contentChar = contentChar + "<td colspan=\"1\">" + sign[k].ToString() + "</td>";
																}
																string pattern = " ";
																XmlNode fill = item.ChildNodes[j].Attributes.GetNamedItem("fill");
																if (fill != null && fill.Value != "")
																{
																	pattern = fill.Value;
																}
																int empty = System.Convert.ToInt32(colspan_save.Value) - sign.Length;
																for (int k = 0; k < empty; k++)
																{
																	contentChar = "<td colspan=\"1\">" + pattern + "</td>" + contentChar;
																}
																contentField += contentChar;
															}
															else
															{
																if (colspan_save != null)
																{
																	contentField = contentField + "<td colspan=\"" + colspan_save.Value + "\">";
																}
																else
																{
																	contentField += "<td>";
																}
																contentField += valueSaveAs.InnerText;
																contentField += "</td>";
															}
														}
														else
														{
															if (colspan_save != null)
															{
																contentField = contentField + "<td colspan=\"" + colspan_save.Value + "\"> </td>";
															}
															else
															{
																contentField += "<td> </td>";
															}
														}
													}
												}
												contentField += "</tr>";
												content += contentField;
												this.additionaContent += contentField;
											}
											else
											{
												contentField += "<p class=\"question\">";
												for (int j = 0; j < item.ChildNodes.Count; j++)
												{
													if (item.ChildNodes[j].Name == "name")
													{
														string num = "";
														if (lp != null && lp.Value != "")
														{
															num = lp.Value + ". ";
														}
														string p = "";
														if (przypis != null && przypis.Value != "")
														{
															p = "<sup>" + przypis.Value + "</sup>";
														}
														string text2 = contentField;
														contentField = string.Concat(new string[]
														{
															text2,
															num,
															p,
															item.ChildNodes[j].InnerXml,
															" "
														});
													}
													if (item.ChildNodes[j].Name == "save_as")
													{
														XmlNode isNewP = item.ChildNodes[j].Attributes.GetNamedItem("newParagraph");
														if (isNewP != null && isNewP.Value == "true")
														{
															contentField += "</p><p class=\"response\">";
														}
														XmlNode valueSaveAs = value.SelectSingleNode(pathSave + "/" + item.ChildNodes[j].InnerText);
														if (valueSaveAs != null)
														{
															contentField += valueSaveAs.InnerXml;
														}
													}
												}
												contentField += "</p>";
												this.additionaContent += contentField;
											}
										}
									}
								}
							}
							this.body += content;
						}
						if (typeFields != null && typeFields.Value == "committee-calculator")
						{
							string content = "";
							XmlNode committee = value.SelectSingleNode("/save");
							string step = "";
							if (stepSave != null)
							{
								step = stepSave.InnerText;
								if (step == "0" || step == "4")
								{
									pathSave = "/save/komisja_sklad";
								}
								committee = value.SelectSingleNode(pathSave);
							}
							foreach (XmlNode item in fields)
							{
								XmlNode type = item.Attributes.GetNamedItem("data");
								XmlNode borderInfo = item.Attributes.GetNamedItem("design");
								bool border = false;
								border = (borderInfo != null && borderInfo.Value == "border");
								stepSave = value.SelectSingleNode("/save/step");
								if (type != null && type.Value == "komisja_info")
								{
									foreach (XmlNode itemChild in item)
									{
										if (itemChild.Name == "name")
										{
											XmlNode bold = itemChild.Attributes.GetNamedItem("bold");
											if (border)
											{
												XmlNode colspan = itemChild.Attributes.GetNamedItem("colspan");
												if (colspan != null)
												{
													content = content + "<tr><td colspan=\"" + colspan.Value + "\">";
												}
												else
												{
													content += "<tr><td>";
												}
												if (bold != null && bold.Value == "true")
												{
													content = content + "<b>" + itemChild.InnerXml + "</b>";
												}
												else
												{
													content += itemChild.InnerXml;
												}
												content += "</td><tr>";
											}
											else
											{
												if (bold != null && bold.Value == "true")
												{
													content = content + "<p><b>" + itemChild.InnerXml + "</b></p>";
												}
												else
												{
													content = content + "<p>" + itemChild.InnerXml + "</p>";
												}
											}
										}
										if (step != "0" && step != "4" && !comitteline)
										{
											content += "<table class=\"noborder\">";
											for (int i = 0; i < 8; i++)
											{
												content += "<tr><td colspan=\"14\"><span class=\"dot\">................................</span>";
												string text = "";
												if (i == 0)
												{
													text = "<br><center>(podpis)</center>";
												}
												content = content + "</td><td colspan=\"10\"><center class=\"dot\">................................</center>" + text + "</td></tr>";
											}
											content += "</table><br>";
											content = content + "<center><img src=\"" + System.IO.Path.GetDirectoryName(Application.StartupPath) + "\\tmp\\printTmp\\stampPlace.png\"></center>";
											comitteline = true;
										}
										if (itemChild.Name == "patternrows")
										{
											for (int i = 0; i < itemChild.ChildNodes.Count; i++)
											{
												XmlNode lp2 = itemChild.ChildNodes[i].Attributes.GetNamedItem("lp");
												int lp3 = 1;
												if (committee != null && stepSave != null && (stepSave.InnerText == "0" || stepSave.InnerText == "4"))
												{
													char[] charsToTrim = new char[]
													{
														' ',
														'.',
														'-',
														':',
														',',
														'_',
														'\n'
													};
													content += "<table class=\"noborder\">";
													foreach (XmlNode person in committee)
													{
														if (person.Attributes.GetNamedItem("obecny") != null && person.Attributes.GetNamedItem("obecny").Value.ToLower() == "true")
														{
															string num = "";
															if (lp2 != null && lp2.Value == "auto")
															{
																num = lp3.ToString() + ") ";
															}
															content = content + "<tr><td colspan=\"14\">" + num;
															if (itemChild.ChildNodes[i].Attributes.GetNamedItem("nazwisko") != null)
															{
																if (person.Attributes.GetNamedItem("nazwisko") != null)
																{
																	content = content + person.Attributes.GetNamedItem("nazwisko").Value + " ";
																}
															}
															if (itemChild.ChildNodes[i].Attributes.GetNamedItem("imie") != null)
															{
																if (person.Attributes.GetNamedItem("imie") != null)
																{
																	content = content + person.Attributes.GetNamedItem("imie").Value + " ";
																}
															}
															if (itemChild.ChildNodes[i].Attributes.GetNamedItem("imie2") != null)
															{
																if (person.Attributes.GetNamedItem("imie2") != null)
																{
																	string imie2 = person.Attributes.GetNamedItem("imie2").Value.Trim(charsToTrim);
																	if (imie2 == "brak")
																	{
																		imie2 = "";
																	}
																	if (imie2 != "")
																	{
																		content = content + imie2 + " ";
																	}
																}
															}
															if (itemChild.ChildNodes[i].Attributes.GetNamedItem("funkcja") != null)
															{
																if (person.Attributes.GetNamedItem("funkcja") != null)
																{
																	string f = person.Attributes.GetNamedItem("funkcja").Value;
																	if (f == "CZ??ONEK")
																	{
																		f = "cz??onek komisji";
																	}
																	else
																	{
																		if (f == "PRZEWODNICZ??CY")
																		{
																			f = "przewodnicz??cy komisji";
																		}
																		else
																		{
																			if (f == "ZAST??PCA")
																			{
																				f = "zast??pca przewodnicz??cego";
																			}
																		}
																	}
																	content = content + f + " ";
																}
															}
															string text = "";
															if (lp3 == 1)
															{
																text = "<br><center>(podpis)</center>";
															}
															content = content + "</td><td colspan=\"10\"><center class=\"dot\">................................</center>" + text + "</td></tr>";
															lp3++;
														}
													}
													content += "</table>";
													content = content + "<center><img src=\"" + System.IO.Path.GetDirectoryName(Application.StartupPath) + "\\tmp\\printTmp\\stampPlace.png\"></center>";
												}
											}
										}
									}
								}
							}
							this.additionaContent += content;
						}
						if (typeFields != null && typeFields.Value == "additional-table")
						{
							tableOfElection = "";
							string content = "";
							if (stepSave != null)
							{
								string step = stepSave.InnerText;
								if (step == "0" || step == "4")
								{
									pathSave = "/save/form";
								}
								if (step == "1" || step == "2" || step == "3")
								{
									pathSave = "/save/step1";
								}
							}
							bool predBorder = false;
							bool border = false;
							foreach (XmlNode item in fields)
							{
								XmlNode borderInfo = item.Attributes.GetNamedItem("design");
								predBorder = border;
								border = (borderInfo != null && borderInfo.Value == "border");
								if (!predBorder && border)
								{
									content += startTable;
								}
								if (predBorder && !border)
								{
									content += endTable;
								}
								if (item.Name == "title")
								{
									XmlNode bold = item.Attributes.GetNamedItem("bold");
									if (border)
									{
										XmlNode colspan = item.Attributes.GetNamedItem("colspan");
										if (colspan != null)
										{
											content = content + "<tr><td colspan=\"" + colspan.Value + "\" class=\"center\">";
										}
										else
										{
											content += "<tr><td>";
										}
										if (bold != null && bold.Value == "true")
										{
											content = content + "<b>" + item.InnerXml + "</b>";
										}
										else
										{
											content += item.InnerXml;
										}
										content += "</td><tr>";
									}
									else
									{
										if (bold != null && bold.Value == "true")
										{
											content = content + "<h1><b>" + item.InnerXml + "</b></h1>";
										}
										else
										{
											content = content + "<h1>" + item.InnerXml + "</h1>";
										}
									}
								}
								if (item.Name == "description")
								{
									XmlNode bold = item.Attributes.GetNamedItem("bold");
									if (border)
									{
										XmlNode colspan = item.Attributes.GetNamedItem("colspan");
										if (colspan != null)
										{
											content = content + "<tr><td colspan=\"" + colspan.Value + "\">";
										}
										else
										{
											content += "<tr><td>";
										}
										if (bold != null && bold.Value == "true")
										{
											content = content + "<b>" + item.InnerXml + "</b>";
										}
										else
										{
											content += item.InnerXml;
										}
										content += "</td><tr>";
									}
									else
									{
										if (bold != null && bold.Value == "true")
										{
											content = content + "<p><b>" + item.InnerXml + "</b></p>";
										}
										else
										{
											content = content + "<p>" + item.InnerXml + "</p>";
										}
									}
								}
								if (item.Name == "note")
								{
									if (item.InnerText != "")
									{
										XmlNode bold = item.Attributes.GetNamedItem("bold");
										if (border)
										{
											XmlNode colspan = item.Attributes.GetNamedItem("colspan");
											if (colspan != null)
											{
												content = content + "<tr><td colspan=\"" + colspan.Value + "\">";
											}
											else
											{
												content += "<tr><td>";
											}
											if (bold != null && bold.Value == "true")
											{
												content = content + "<b>Uwaga!</b><b>" + item.InnerXml + "</b>";
											}
											else
											{
												content += item.InnerXml;
											}
											content += "</td><tr>";
										}
										else
										{
											if (bold != null && bold.Value == "true")
											{
												content = content + "<table class=\"noborder\" ><tr><td colspan=\"2\"><b>Uwaga!</b></td><td colspan=\"22\"><b>" + item.InnerXml + "</b></td></tr></table>";
											}
											else
											{
												content = content + "<table class=\"noborder\"><tr><td colspan=\"2\"><b>Uwaga!</b></td><td colspan=\"22\">" + item.InnerXml + "</td></tr></table>";
											}
										}
									}
									else
									{
										if (border)
										{
											XmlNode colspan = item.Attributes.GetNamedItem("colspan");
											if (colspan != null)
											{
												content = content + "<tr><td colspan=\"" + colspan.Value + "\">";
											}
											else
											{
												content += "<tr><td>";
											}
											content += " <td></tr>";
										}
										else
										{
											content += "<p></p>";
										}
									}
								}
								if (item.Name == "field")
								{
									string contentField = "";
									XmlNode lp = item.Attributes.GetNamedItem("lp");
									if (border)
									{
										contentField += "<tr>";
										XmlNode colspan = item.Attributes.GetNamedItem("colspan");
										XmlNode colspan_save = item.Attributes.GetNamedItem("colspan_save");
										XmlNode colspan_nr = item.Attributes.GetNamedItem("colspan_nr");
										if (lp != null && lp.Value != "" && colspan_nr != null && colspan_nr.Value != "" && System.Convert.ToInt32(colspan_nr.Value) > 0)
										{
											string text2 = contentField;
											contentField = string.Concat(new string[]
											{
												text2,
												"<td colspan =\"",
												colspan_nr.Value,
												"\" class=\"lp\">",
												lp.Value,
												". </td>"
											});
										}
										for (int j = 0; j < item.ChildNodes.Count; j++)
										{
											string text = item.ChildNodes[j].InnerXml.Replace(System.Environment.NewLine, "<br>");
											if (item.ChildNodes[j].Name == "name" && colspan != null && colspan.Value != "" && System.Convert.ToInt32(colspan.Value) > 0)
											{
												string text2 = contentField;
												contentField = string.Concat(new string[]
												{
													text2,
													"<td colspan =\"",
													colspan.Value,
													"\">",
													text,
													"</td>"
												});
											}
											if (item.ChildNodes[j].Name == "save_as")
											{
												XmlNode ischar = item.ChildNodes[j].Attributes.GetNamedItem("char");
												XmlNode valueSaveAs = value.SelectSingleNode(pathSave + "/" + item.ChildNodes[j].InnerText);
												if (valueSaveAs != null)
												{
													if (ischar != null && ischar.Value == "true" && colspan_save != null && colspan_save.Value != "" && System.Convert.ToInt32(colspan_save.Value) > 0 && valueSaveAs.InnerText.Length <= System.Convert.ToInt32(colspan_save.Value))
													{
														char[] sign = valueSaveAs.InnerText.ToCharArray();
														string contentChar = "";
														for (int k = 0; k < sign.Length; k++)
														{
															contentChar = contentChar + "<td colspan=\"1\">" + sign[k].ToString() + "</td>";
														}
														string pattern = " ";
														XmlNode fill = item.ChildNodes[j].Attributes.GetNamedItem("fill");
														if (fill != null && fill.Value != "")
														{
															pattern = fill.Value;
														}
														int empty = System.Convert.ToInt32(colspan_save.Value) - sign.Length;
														for (int k = 0; k < empty; k++)
														{
															contentChar = "<td colspan=\"1\">" + pattern + "</td>" + contentChar;
														}
														contentField += contentChar;
													}
													else
													{
														if (colspan_save != null)
														{
															contentField = contentField + "<td colspan=\"" + colspan_save.Value + "\">";
														}
														else
														{
															contentField += "<td>";
														}
														contentField += valueSaveAs.InnerText;
														contentField += "</td>";
													}
												}
												else
												{
													if (colspan_save != null)
													{
														contentField = contentField + "<td colspan=\"" + colspan_save.Value + "\"> </td>";
													}
													else
													{
														contentField += "<td> </td>";
													}
												}
											}
										}
										contentField += "</tr>";
									}
									else
									{
										contentField += "<p>";
										if (lp != null && lp.Value != "")
										{
											contentField = contentField + lp.Value + " ";
										}
										for (int j = 0; j < item.ChildNodes.Count; j++)
										{
											if (item.ChildNodes[j].Name == "name")
											{
												contentField = contentField + item.ChildNodes[j].InnerXml + " ";
											}
											if (item.ChildNodes[j].Name == "save_as")
											{
												XmlNode valueSaveAs = value.SelectSingleNode(pathSave + "/" + item.ChildNodes[j].InnerText);
												if (valueSaveAs != null)
												{
													contentField += valueSaveAs.Value;
												}
											}
										}
										contentField += "</p>";
									}
									content += contentField;
								}
							}
							if (!predBorder && border)
							{
								content += startTable;
							}
							if (predBorder && !border)
							{
								content += endTable;
							}
							tableOfElection += content;
						}
					}
					if (fields.Name == "przypis")
					{
						string content = "<p class=\"sup\">";
						XmlNode name = fields.Attributes.GetNamedItem("name");
						if (name != null)
						{
							string text2 = content;
							content = string.Concat(new string[]
							{
								text2,
								"<sup>",
								name.Value,
								"</sup>",
								fields.InnerXml
							});
						}
						content += "</p>";
						this.additionaContent += content;
					}
				}
				System.Collections.Generic.List<string>[] errorsList = this.getError(value);
				try
				{
					this.raport += "</table>";
					this.raport += "<h1><b>RAPORT OSTRZE??E?? W PROTOKOLE G??OSOWANIA</b></h1>";
					if (errorsList.Length == 2)
					{
						if (errorsList[0].Count > 0)
						{
							this.raport += "<h1><b>Ostrze??enia blokuj??ce wydruk</b></h1>";
							for (int r3 = 0; r3 < errorsList[0].Count; r3++)
							{
								string text2 = this.raport;
								this.raport = string.Concat(new string[]
								{
									text2,
									"<p><b>",
									errorsList[0][r3],
									"</b> ",
									this.errors[errorsList[0][r3]],
									"</p>"
								});
							}
						}
						if (errorsList[1].Count > 0)
						{
							this.raport += "<h1><b>Ostrze??enia</b></h1>";
							for (int r3 = 0; r3 < errorsList[1].Count; r3++)
							{
								string text2 = this.raport;
								this.raport = string.Concat(new string[]
								{
									text2,
									"<p><b>",
									errorsList[1][r3],
									"</b> ",
									this.errors[errorsList[1][r3]],
									"</p>"
								});
							}
						}
						this.raport += "<p></p><p> Stanowisko Komisji:</p>";
						this.raport += "<p class=\"dot\">..... ..... ..... ..... ..... ..... ..... ..... ..... ..... ..... ..... ..... ..... ..... ..... ..... ..... ..... ..... ..... ..... ..... ..... ..... ..... ..... ..... ..... ..... ..... ..... ..... ..... ..... ..... ..... ..... ..... ..... ..... ..... ..... ..... .....</p>";
						this.raport += "<p> Przy sporz??dzeniu raportu ostrze??e?? byli cz??onkowie Komisji:</p>";
						this.raport += "<p class=\"dot\"> ........................................ ........................................<br>";
						this.raport += "........................................ ........................................<br>";
						this.raport += "........................................ ........................................<br>";
						this.raport += "........................................ ........................................</p>";
						this.raport += "<div id=\"breakTab1\" class=\"break\"> &#8194 </div>";
					}
				}
				catch (System.Exception)
				{
				}
				string t = "<br><br><br><br><br><br><table class=\"noborder\"><tr><td colspan=\"14\"></td><td colspan=\"10\"><center class=\"dot\">................................</center> <br><center>(podpis przewodnicz??cego / zast??pcy przewodnicz??cego<sup>*</sup>) komisji ??? osoby, kt??rej nazwisko zosta??o podane w pkt 3 zestawienia)</center></td></tr></table><br><br><br><br><br><br><br><sup>*</sup> Zb??dne pomin????";
				stepSave = value.SelectSingleNode("/save/step");
				if (stepSave != null && stepSave.InnerText == "0")
				{
					this.body += this.converToPage();
					tableOfElection = tableOfElection + t + "<div id=\"breakTab\" class=\"break\"> &#8194 </div>";
					if (errorsList[0].Count > 0 || errorsList[1].Count > 0)
					{
						tableOfElection += this.raport;
					}
					this.body = tableOfElection + this.body;
				}
				else
				{
					this.body += this.additionaContent;
					tableOfElection += "<div id=\"breakTab\" class=\"break\"> &#8194 </div>";
					if (errorsList[0].Count > 0 || errorsList[1].Count > 0)
					{
						tableOfElection += this.raport;
					}
					this.body = tableOfElection + this.body;
				}
			}
			catch (XmlException)
			{
				MessageBox.Show("Nieprawid??owy XML", "B????d");
			}
			catch (System.NullReferenceException)
			{
				MessageBox.Show("Podanno inny xml niz header", "B????d");
			}
			return response;
		}
		public System.Collections.Generic.List<string>[] getError(XmlDocument value)
		{
			System.Collections.Generic.List<string>[] errors = new System.Collections.Generic.List<string>[2];
			XmlNode hardWarning = value.SelectSingleNode("/save/hardWarning");
			XmlNode softWarning = value.SelectSingleNode("/save/softError");
			errors[0] = new System.Collections.Generic.List<string>();
			if (hardWarning != null)
			{
				foreach (XmlNode field in hardWarning)
				{
					for (int i = 0; i < field.ChildNodes.Count; i++)
					{
						bool can = true;
						for (int j = 0; j < errors[0].Count; j++)
						{
							if (field.ChildNodes[i].InnerText == errors[0][j])
							{
								can = false;
								break;
							}
						}
						if (can)
						{
							errors[0].Add(field.ChildNodes[i].InnerText);
						}
					}
				}
			}
			if (errors[0].Count > 1)
			{
				errors[0].Sort();
			}
			errors[1] = new System.Collections.Generic.List<string>();
			if (softWarning != null)
			{
				foreach (XmlNode field in softWarning)
				{
					for (int i = 0; i < field.ChildNodes.Count; i++)
					{
						bool can = true;
						for (int j = 0; j < errors[1].Count; j++)
						{
							if (field.ChildNodes[1].InnerText == errors[1][j])
							{
								can = false;
								break;
							}
						}
						if (can)
						{
							errors[1].Add(field.ChildNodes[i].InnerText);
						}
					}
				}
			}
			if (errors[1].Count > 1)
			{
				errors[1].Sort();
			}
			return errors;
		}
		private void setErrorsTab()
		{
			try
			{
				this.errors = new System.Collections.Generic.Dictionary<string, string>();
				this.errors.Add("INW", "Nie wype??nione pole/a \"Imi??\"");
				this.errors.Add("NNW", "Nie wype??nione pole/a \"Nazwisko\"");
				this.errors.Add("ErrorNull", "Pole puste");
				this.errors.Add("ErrorType", "Nie prawid??owy format danych");
				this.errors.Add("ErrorEqual", "Odpowiednie pola z kroku 1 i 2 nie s?? sobie r??wne");
				this.errors.Add("SNT02", "Liczba kart do g??osowania wydanych wyborcom (z pkt 3) nie mo??e by?? wi??ksza od liczby wyborc??w uprawnionych do g??osowania (wpisanych do spisu wyborc??w wed??ug stanu w chwili zako??czenia g??osowania ??? z pkt 1)");
				this.errors.Add("SNT02a", "W przypadku obwodu wyznaczonego dla g??osowania korespondencyjnego suma liczby kart do g??osowania wydanych wyborcom (pkt 3) i liczby wys??anych pakiet??w wyborczych (pkt 1a) nie mo??e by?? wi??ksza od liczby wyborc??w uprawnionych do g??osowania (wpisanych do spisu wyborc??w wed??ug stanu w chwili zako??czenia g??osowania ??? pkt 1).");
				this.errors.Add("SNT03", "Liczba wyborc??w g??osuj??cych przez pe??nomocnika (z pkt 3a) nie mo??e by?? wi??ksza od liczby kart do g??osowania wydanych wyborcom (z pkt 3)");
				this.errors.Add("SNT03a", "W przypadku obwod??w g??osowania innych ni?? sta??e utworzone w kraju liczba wyborc??w g??osuj??cych przez pe??nomocnika (z pkt 3a) musi wynosi?? 0.");
				this.errors.Add("SNT04", "Liczba wys??anych pakiet??w wyborczych (pkt 1a) nie mo??e by?? wi??ksza od liczby kart do g??osowania wydanych wyborcom (pkt 3). Nie dotyczy obwod??w utworzonych za granic??.");
				this.errors.Add("SNT04a", "Liczba wys??anych pakiet??w wyborczych (pkt 1a) w przypadku obwodowych komisji wyborczych niewyznaczonych dla g??osowania korespondencyjnego musi wynosi?? 0.");
				this.errors.Add("SNT05", "Suma liczb z pkt. 6. i 7. <b>musi</b> by?? r??wna liczbie z pkt. 5.");
				this.errors.Add("SNT06", "Suma liczb z pkt. 8. i 9. <b>musi</b> by?? r??wna liczbie z pkt. 7.");
				this.errors.Add("SNT07", "Liczba g??os??w wa??nych (pkt 9) musi by?? r??wna sumie liczb g??os??w wa??nych ????cznie oddanych na wszystkich kandydat??w ??? w przypadku g??osowania na wi??cej ni?? jednego kandydata.");
				this.errors.Add("SNT08", "Warto???? w rubryce ???Razem??? w pkt 10 protoko??u musi by?? r??wna sumie liczb g??os??w oddanych na kandydat??w.");
				this.errors.Add("SNT10a", "W obwodach nieobj??tych g??osowaniem korespondencyjnym liczby w zestawieniu dotycz??ce g??osowania korespondencyjnego (z zestawienia 5) musz?? by?? r??wne 0.");
				this.errors.Add("SNT10b", "W obwodach nieobj??tych g??osowaniem korespondencyjnym liczby w zestawieniu dotycz??ce g??osowania korespondencyjnego (z zestawienia 5) musz?? by?? r??wne 0.");
				this.errors.Add("SNT10c", "W obwodach nieobj??tych g??osowaniem korespondencyjnym liczby w zestawieniu dotycz??ce g??osowania korespondencyjnego (z zestawienia 5) musz?? by?? r??wne 0.");
				this.errors.Add("SNT10d", "W obwodach nieobj??tych g??osowaniem korespondencyjnym liczby w zestawieniu dotycz??ce g??osowania korespondencyjnego (z zestawienia 5) musz?? by?? r??wne 0.");
				this.errors.Add("SNT10e", "W obwodach nieobj??tych g??osowaniem korespondencyjnym liczby w zestawieniu dotycz??ce g??osowania korespondencyjnego (z zestawienia 5) musz?? by?? r??wne 0.");
				this.errors.Add("SNT10f", "W obwodach nieobj??tych g??osowaniem korespondencyjnym liczby w zestawieniu dotycz??ce g??osowania korespondencyjnego (z zestawienia 5) musz?? by?? r??wne 0.");
				this.errors.Add("SNT11", "Suma liczby os??b g??osuj??cych na podstawie za??wiadczenia (z zestawienia 4) i zawartej w protokole liczby os??b g??osuj??cych przez pe??nomocnika (z pkt 3a) nie mo??e by?? wi??ksza od liczby wydanych kart do g??osowania (z pkt 3)");
				this.errors.Add("SNT12", "Suma liczby kopert zwrotnych bez o??wiadczenia (z za??wiadczenia 6) i liczby kopert z niepodpisanym za??wiadczeniem (z zestawienia 7) nie mo??e by?? wi??ksza od liczby otrzymanych kopert zwrotnych (z zestawienia 5).");
				this.errors.Add("SNT13", "Suma liczby kopert zwrotnych bez koperty na kart?? do g??osowania (z zestawienia 8) oraz liczby kopert zwrotnych z niezaklejon?? kopert?? na kart?? do g??osowania (z zestawienia 9) nie mo??e by?? wi??ksza od liczby otrzymanych kopert zwrotnych (z zestawienia 5).");
				this.errors.Add("SNT14", "Liczba kopert na karty do g??osowania wrzuconych do urny nie mo??e by?? wi??ksza ni?? liczba otrzymanych kopert zwrotnych pomniejszona o liczb?? kopert zwrotnych nie spe??niaj??cych warunk??w");
				this.errors.Add("SNTO01", "Liczba wyborc??w w obwodzie sta??ym, bez os??b dopisanych do spisu na podstawie za??wiadczenia o prawie do g??osowania (czyli pkt 1 protoko??u minus pkt 4 zestawienia) nie powinna by?? wi??ksza od 110% szacowanej liczby wyborc??w (liczby wyborc??w ustalonej w meldunku wyborczym).");
				this.errors.Add("SNTO01a", "Liczba wyborc??w w obwodzie sta??ym, bez os??b dopisanych do spisu na podstawie za??wiadczenia o prawie do g??osowania (czyli pkt 1 protoko??u minus pkt 4 zestawienia) nie powinna by?? mniejsza od 90% szacowanej liczby wyborc??w (liczby wyborc??w ustalonej w meldunku wyborczym).");
				this.errors.Add("SNTO02", "Liczba uprawnionych do g??osowania (z pkt 1) nie powinna by?? mniejsza ni?? liczba kart wa??nych (z pkt 7)");
				this.errors.Add("SNTO03", "Suma liczby os??b g??osuj??cych przez pe??nomocnika (pkt 3a) i liczby os??b g??osuj??cych na podstawie za??wiadczenia (pkt 4 zestawienia) nie powinna by?? r??wna liczbie wydanych kart do g??osowania (pkt 3).");
				this.errors.Add("SNTO04", "Suma liczby wydanych przez komisj?? kart do g??osowania (z pkt 3) i liczby kart niewykorzystanych (z pkt 4) nie powinna by?? wi??ksza od liczby kart do g??osowania otrzymanych przez komisj?? (z pkt 2)");
				this.errors.Add("SNTO05", "Liczba kart niewa??nych (pkt 6) jest wi??ksza od 0, a liczba g??os??w niewa??nych (pkt 8) r??wna jest 0. Prawdopodobnie g??osy niewa??ne zosta??y wpisane jako karty niewa??ne");
				this.errors.Add("SNTO06", "W obwodzie niekorespondencyjnym liczba kart wyj??tych z urny (pkt 5) nie powinna by?? wi??ksza od liczby wyborc??w, kt??rym wydano karty do g??osowania (pkt 3).");
				this.errors.Add("SNTO06a", "W obwodzie korespondencyjnym liczba kart wyj??tych z urny (pkt 5) pomniejszona o liczb?? kart wyj??tych z kopert na karty do g??osowania (pkt 5a) nie powinna by?? wi??ksza od liczby wyborc??w, kt??rym wydano karty do g??osowania (pkt 3).");
				this.errors.Add("SNTO07", "Liczba otrzymanych kart do g??osowania (z pkt 2) w obwodach sta??ych utworzonych w kraju nie powinna odbiega?? o wi??cej ni?? 10 punkt??w procentowych od przewidywanego nak??adu kart do g??osowania.");
				this.errors.Add("SNTO07a", "Liczba otrzymanych kart do g??osowania (z pkt 2) w obwodach sta??ych utworzonych w kraju nie powinna odbiega?? o wi??cej ni?? 10 punkt??w procentowych od przewidywanego nak??adu kart do g??osowania.");
				this.errors.Add("SNTO12", "Liczba otrzymanych kopert zwrotnych (z zestawienia 5) nie powinna by?? wi??ksza, ni?? liczba wys??anych pakiet??w wyborczych (z pkt 1a).");
				this.errors.Add("SNTO13", "Liczba kart wyj??tych z kopert na karty do g??osowania (z pkt 5a) nie powinna by?? wi??ksza od liczby wyborc??w, kt??rym wys??ano pakiety wyborcze (z pkt 1a).");
				this.errors.Add("SNTO14", "Liczba kart do g??osowania wyj??tych z kopert na karty do g??osowania (z pkt 5a) nie powinna by?? wi??ksza od liczby kopert na kart?? do g??osowania wrzuconych do urny (z za??wiadczenia 10)");
				this.errors.Add("SNM01", "W obwodzie sta??ym liczba kart wydanych (pkt 3) nie powinna przekracza?? 90% liczby wyborc??w (pkt 1).");
				this.errors.Add("SNM01a", "W obwodzie sta??ym przeznaczonym do g??osowania korespondencyjnego suma liczby kart wydanych (pkt 3) i wys??anych pakiet??w wyborczych (pkt 1a) nie powinna przekracza?? 90% liczby wyborc??w (pkt 1).");
				this.errors.Add("SNM02", "Liczba uprawnionych do g??osowania (z pkt 1) powinna by?? niezerowa");
				this.errors.Add("SNM03", "Liczba kart wydanych (z pkt 3) powinna by?? niezerowa.");
				this.errors.Add("SNM04", "Liczba kart wyj??tych z urny (z pkt 5) powinna by?? niezerowa.");
				this.errors.Add("SNM05", "Liczba kart wa??nych (z pkt 7) powinna by?? niezerowa.");
				this.errors.Add("SNM06", "Liczba kart wa??nych wyj??tych z urny (z pkt 7) nie powinna by?? wi??ksza od liczby wyborc??w uprawnionych do g??osowania (z pkt 1).");
				this.errors.Add("SNM07", "Liczba kart wa??nych (z pkt 7) powinna by?? r??wna liczbie os??b, kt??rym wydano karty (z pkt 3)");
				this.errors.Add("SNM07a", "W obwodzie korespondencyjnym liczba kart wa??nych (pkt 7) pomniejszona o liczb?? wys??anych pakiet??w wyborczych (pkt 1a) powinna by?? r??wna liczbie os??b, kt??rym wydano karty (pkt 3)");
				this.errors.Add("SNM08", "Liczba kart niewa??nych (z pkt 6) nie powinna by?? wi??ksza od zera");
				this.errors.Add("SNM09", "Protok???? obwodowy powinien mie?? wprowadzony sk??ad obwodowej komisji wyborczej");
				this.errors.Add("SNM010", "Liczba cz??onk??w komisji  (wliczaj??c przewodnicz??cego i zast??pc??) powinna mie??ci?? si?? w ustawowych wide??kach.");
				this.errors.Add("SNM011", "Komisja powinna mie?? przewodnicz??cego");
				this.errors.Add("SNM012", "Komisja powinna mie?? wiceprzewodnicz??cego");
				this.errors.Add("SNM013", "Protok???? powinna podpisa?? przynajmniej po??owa sk??adu komisji");
				this.errors.Add("SNM014", "Protok???? powinien podpisa?? przewodnicz??cy b??d?? zast??pca");
			}
			catch (System.Exception)
			{
			}
		}
		private System.Collections.Generic.List<Field> readPatternCandidate(XmlNode itemChild, System.Collections.Generic.List<Field> patternField)
		{
			foreach (XmlNode nodeItem in itemChild)
			{
				if (nodeItem.Name == "fildpatern")
				{
					XmlNode name = nodeItem.Attributes.GetNamedItem("name");
					XmlNode name2 = nodeItem.Attributes.GetNamedItem("name2");
					XmlNode name3 = nodeItem.Attributes.GetNamedItem("name2v2");
					XmlNode p = nodeItem.Attributes.GetNamedItem("plec");
					XmlNode state = nodeItem.Attributes.GetNamedItem("status");
					XmlNode imie = nodeItem.Attributes.GetNamedItem("imie");
					XmlNode imie2 = nodeItem.Attributes.GetNamedItem("imie2");
					XmlNode nazwisko = nodeItem.Attributes.GetNamedItem("nazwisko");
					XmlNode dataType = nodeItem.Attributes.GetNamedItem("data");
					XmlNode komitet = nodeItem.Attributes.GetNamedItem("komitet-wyborczy");
					XmlNode save_as = nodeItem.Attributes.GetNamedItem("save_as");
					XmlNode ischar = nodeItem.Attributes.GetNamedItem("char");
					XmlNode fill = nodeItem.Attributes.GetNamedItem("fill");
					XmlNode display = nodeItem.Attributes.GetNamedItem("display");
					string n = "";
					if (name != null)
					{
						n = name.Value;
					}
					string n2 = "";
					if (name2 != null)
					{
						n2 = name2.Value;
					}
					string n3 = "";
					if (name3 != null)
					{
						n3 = name3.Value;
					}
					string s = "";
					if (state != null)
					{
						s = state.Value;
					}
					string p2 = "";
					if (p != null)
					{
						p2 = p.Value;
					}
					string i = "";
					if (imie != null)
					{
						i = imie.Value;
					}
					string i2 = "";
					if (imie2 != null)
					{
						i2 = imie2.Value;
					}
					string naz = "";
					if (nazwisko != null)
					{
						naz = nazwisko.Value;
					}
					string dt = "";
					if (dataType != null)
					{
						dt = dataType.Value;
					}
					string sa = "";
					if (save_as != null)
					{
						sa = save_as.Value;
					}
					string j = "";
					if (komitet != null)
					{
						j = komitet.Value;
					}
					string dis = "";
					if (display != null)
					{
						dis = display.Value;
					}
					string @char = "";
					if (ischar != null)
					{
						@char = ischar.Value;
					}
					string fill2 = "";
					if (fill != null)
					{
						fill2 = fill.Value;
					}
					patternField.Add(new Field(n, n2, n3, p2, s, i, i2, naz, dt, sa, j, @char, fill2, dis));
				}
			}
			return patternField;
		}
		private string getImie2BasePatternCandidate(Field patternFieldI, XmlNode lista)
		{
			string imie2 = "";
			if (patternFieldI.getImie2() == patternFieldI.getImie2().Replace("parent:", ""))
			{
				if (lista.FirstChild.Attributes.GetNamedItem(patternFieldI.getImie2()) != null)
				{
					imie2 = lista.FirstChild.Attributes.GetNamedItem(patternFieldI.getImie2()).Value + " ";
				}
			}
			else
			{
				string label = patternFieldI.getImie2().Replace("parent:", "");
				if (lista.Attributes.GetNamedItem(label) != null)
				{
					imie2 = lista.Attributes.GetNamedItem(label).Value + " ";
				}
			}
			return imie2;
		}
		private string getImie1BasePatternCandidate(Field patternFieldI, XmlNode lista)
		{
			string imie = "";
			if (patternFieldI.getImie1() == patternFieldI.getImie1().Replace("parent:", ""))
			{
				if (lista.FirstChild.Attributes.GetNamedItem(patternFieldI.getImie1()) != null)
				{
					imie = lista.FirstChild.Attributes.GetNamedItem(patternFieldI.getImie1()).Value + " ";
				}
			}
			else
			{
				string label = patternFieldI.getImie1().Replace("parent:", "");
				if (lista.Attributes.GetNamedItem(label) != null)
				{
					imie = lista.Attributes.GetNamedItem(label).Value + " ";
				}
			}
			return imie;
		}
		private string getNazwiskoBasePatternCandidate(Field patternFieldI, XmlNode lista)
		{
			string nazwisko = "";
			if (patternFieldI.getNazwisko() == patternFieldI.getNazwisko().Replace("parent:", ""))
			{
				if (lista.FirstChild.Attributes.GetNamedItem(patternFieldI.getNazwisko()) != null)
				{
					nazwisko = lista.FirstChild.Attributes.GetNamedItem(patternFieldI.getNazwisko()).Value + " ";
				}
			}
			else
			{
				string label = patternFieldI.getNazwisko().Replace("parent:", "");
				if (lista.Attributes.GetNamedItem(label) != null)
				{
					nazwisko = lista.Attributes.GetNamedItem(label).Value + " ";
				}
			}
			return nazwisko;
		}
		private string getKomitetBasePatternCandidate(Field patternFieldI, XmlNode lista)
		{
			string komitet = "";
			if (patternFieldI.getKomitet() == patternFieldI.getKomitet().Replace("parent:", ""))
			{
				if (lista.FirstChild.Attributes.GetNamedItem(patternFieldI.getKomitet()) != null)
				{
					komitet = lista.FirstChild.Attributes.GetNamedItem(patternFieldI.getKomitet()).Value;
				}
			}
			else
			{
				string label = patternFieldI.getKomitet().Replace("parent:", "");
				if (lista.Attributes.GetNamedItem(label) != null)
				{
					komitet = lista.Attributes.GetNamedItem(label).Value;
				}
			}
			return komitet;
		}
		private string getName2BasePatternCandidate(Field patternFieldI, XmlNode lista)
		{
			string name2 = "";
			if (patternFieldI.getPlec() == patternFieldI.getPlec().Replace("parent:", ""))
			{
				if (lista.FirstChild.Attributes.GetNamedItem(patternFieldI.getPlec()) != null)
				{
					if (lista.FirstChild.Attributes.GetNamedItem(patternFieldI.getPlec()).Value.ToUpper() == "M")
					{
						name2 = patternFieldI.getName2() + " ";
					}
					else
					{
						name2 = patternFieldI.getName2v2() + " ";
					}
				}
			}
			else
			{
				string label = patternFieldI.getPlec().Replace("parent:", "");
				if (lista.Attributes.GetNamedItem(label) != null)
				{
					if (lista.Attributes.GetNamedItem(label).Value.ToUpper() == "M")
					{
						name2 = patternFieldI.getName2() + " ";
					}
					else
					{
						name2 = patternFieldI.getName2v2() + " ";
					}
				}
				else
				{
					if (patternFieldI.getName2() != "")
					{
						name2 = patternFieldI.getName2() + " ";
					}
				}
			}
			return name2;
		}
		private string getStatusBasePatternCandidate(Field patternFieldI, XmlNode lista)
		{
			string status = "A";
			if (patternFieldI.getStatus() == patternFieldI.getStatus().Replace("parent:", ""))
			{
				if (lista.FirstChild.Attributes.GetNamedItem(patternFieldI.getStatus()) != null)
				{
					status = lista.FirstChild.Attributes.GetNamedItem(patternFieldI.getStatus()).Value;
				}
			}
			else
			{
				string label = patternFieldI.getStatus().Replace("parent:", "");
				if (lista.Attributes.GetNamedItem(label) != null)
				{
					status = lista.Attributes.GetNamedItem(label).Value;
				}
			}
			return status;
		}
		private string converToPage()
		{
			string response = "";
			int countLine = 50;
			int countSign = 100;
			int sign = 0;
			int line = 1;
			string meta = "";
			bool isMeta = false;
			bool isQuestion = false;
			string result;
			for (int i = 0; i < this.additionaContent.Length; i++)
			{
				char a = this.additionaContent[i];
				if (this.additionaContent[i] == '\n')
				{
					response += "<br>";
					line++;
					sign = 0;
				}
				if (this.additionaContent[i] == '<')
				{
					response += this.additionaContent[i];
					isMeta = true;
					meta = this.additionaContent[i].ToString();
				}
				else
				{
					if (this.additionaContent[i] == '>')
					{
						isMeta = false;
						response += this.additionaContent[i];
						meta += this.additionaContent[i].ToString();
						if (meta == "<br>")
						{
							line++;
							sign = 0;
						}
						if (meta == "<li>")
						{
							line += 2;
							sign = 0;
							isQuestion = false;
						}
						if (System.Text.RegularExpressions.Regex.IsMatch(meta, "^\\<p"))
						{
							line += 2;
							sign = 0;
							string p = "<p class=\"question\">";
							isQuestion = (meta == p);
						}
						if (meta == "</tr>")
						{
							line++;
							sign = 0;
							isQuestion = false;
						}
						if (System.Text.RegularExpressions.Regex.IsMatch(meta, "^\\<img"))
						{
							line += 10;
							sign = 0;
							isQuestion = false;
							if (line >= countLine)
							{
								response += "<br><br><br><br><br><br><br>";
								line = 1;
								sign = 0;
								sign++;
							}
						}
						if (System.Text.RegularExpressions.Regex.IsMatch(meta, "^\\<img class="))
						{
							sign = 0;
							isQuestion = false;
						}
						if (meta == "<script>")
						{
							response += this.additionaContent.Substring(i);
							result = response;
							return result;
						}
					}
					else
					{
						if (isMeta)
						{
							meta += this.additionaContent[i].ToString();
							response += this.additionaContent[i];
						}
						else
						{
							if (line == countLine)
							{
								response += "<br><br><br><br><br><br><br>";
								line = 1;
								sign = 0;
								response += this.additionaContent[i];
								sign++;
							}
							else
							{
								response += this.additionaContent[i];
								sign++;
								if (sign == countSign)
								{
									if (this.additionaContent[i] != ' ')
									{
										i++;
										while (this.additionaContent[i] != ' ' && !isMeta && this.additionaContent[i + 1] != '<')
										{
											if (this.additionaContent[i] == '<')
											{
												isMeta = true;
											}
											if (this.additionaContent[i] == '>')
											{
												isMeta = false;
											}
											response += this.additionaContent[i];
											i++;
										}
										response += this.additionaContent[i];
										response += "<br>";
										line++;
										sign = 0;
										if (isQuestion)
										{
											response += "&#8194 &#8194 &#8194 ";
											sign = 5;
										}
										if (line == countLine)
										{
											response += "<br><br><br><br><br><br><br>";
											line = 1;
											sign = 0;
										}
									}
									else
									{
										response += "<br>";
										line++;
										sign = 0;
										if (isQuestion)
										{
											response += "&#8194 &#8194 &#8194 ";
											sign = 5;
										}
										if (line == countLine)
										{
											response += "<br><br><br><br><br><br><br>";
											line = 1;
											sign = 0;
										}
									}
								}
							}
						}
					}
				}
			}
			result = response;
			return result;
		}
		public string getPage()
		{
			string result;
			try
			{
				if (!System.IO.Directory.Exists(this.path + "\\tmp"))
				{
					try
					{
						System.IO.Directory.CreateDirectory(this.path + "\\tmp");
					}
					catch (System.ArgumentNullException)
					{
						MessageBox.Show("Nieprawid??owa ??cie??ka. Nie mo??na utworzy?? katalogu \"tmp\"", "Error");
					}
					catch (System.ArgumentException)
					{
						MessageBox.Show("Nieprawid??owa ??cie??ka. Nie mo??na utworzy?? katalogu \"tmp\"", "Error");
					}
					catch (System.UnauthorizedAccessException)
					{
						MessageBox.Show("Nie masz uprawnie?? do tworzenia katalog??w. Otw??rz aplikacje jako adnimistrator.", "Uwaga");
					}
					catch (System.IO.PathTooLongException)
					{
						MessageBox.Show("Nieprawid??owa ??cie??ka. Nie mo??na utworzy?? katalogu \"tmp\"", "Error");
					}
					catch (System.IO.DirectoryNotFoundException)
					{
						MessageBox.Show("Nieprawid??owa ??cie??ka. Nie mo??na utworzy?? katalogu \"tmp\"", "Error");
					}
					catch (System.NotSupportedException)
					{
						MessageBox.Show("Nieprawid??owy format ??cie??ki. Nie mo??na utworzy?? katalogu \"tmp\"", "Error");
					}
					catch (System.IO.IOException)
					{
						MessageBox.Show("Nieprawid??owa ??cie??ka. Nie mo??na utworzy?? katalogu \"tmp\"", "Error");
					}
				}
				if (!System.IO.Directory.Exists(this.path + "\\tmp\\printTmp"))
				{
					try
					{
						System.IO.Directory.CreateDirectory(this.path + "\\tmp\\printTmp");
					}
					catch (System.ArgumentNullException)
					{
						MessageBox.Show("Nieprawid??owa ??cie??ka. Nie mo??na utworzy?? katalogu \"printTmp\"", "Error");
					}
					catch (System.ArgumentException)
					{
						MessageBox.Show("Nieprawid??owa ??cie??ka. Nie mo??na utworzy?? katalogu \"printTmp\"", "Error");
					}
					catch (System.UnauthorizedAccessException)
					{
						MessageBox.Show("Nie masz uprawnie?? do tworzenia katalog??w. Otw??rz aplikacje jako adnimistrator.", "Uwaga");
					}
					catch (System.IO.PathTooLongException)
					{
						MessageBox.Show("Nieprawid??owa ??cie??ka. Nie mo??na utworzy?? katalogu \"printTmp\"", "Error");
					}
					catch (System.IO.DirectoryNotFoundException)
					{
						MessageBox.Show("Nieprawid??owa ??cie??ka. Nie mo??na utworzy?? katalogu \"printTmp\"", "Error");
					}
					catch (System.NotSupportedException)
					{
						MessageBox.Show("Nieprawid??owy format ??cie??ki. Nie mo??na utworzy?? katalogu \"printTmp\"", "Error");
					}
					catch (System.IO.IOException)
					{
						MessageBox.Show("Nieprawid??owa ??cie??ka. Nie mo??na utworzy?? katalogu \"printTmp\"", "Error");
					}
				}
				if (!System.IO.Directory.Exists(this.path + "\\tmp\\printTmp\\css"))
				{
					try
					{
						System.IO.Directory.CreateDirectory(this.path + "\\tmp\\printTmp\\css");
					}
					catch (System.ArgumentNullException)
					{
						MessageBox.Show("Nieprawid??owa ??cie??ka. Nie mo??na utworzy?? katalogu \"css\"", "Error");
					}
					catch (System.ArgumentException)
					{
						MessageBox.Show("Nieprawid??owa ??cie??ka. Nie mo??na utworzy?? katalogu \"css\"", "Error");
					}
					catch (System.UnauthorizedAccessException)
					{
						MessageBox.Show("Nie masz uprawnie?? do tworzenia katalog??w. Otw??rz aplikacje jako adnimistrator.", "Uwaga");
					}
					catch (System.IO.PathTooLongException)
					{
						MessageBox.Show("Nieprawid??owa ??cie??ka. Nie mo??na utworzy?? katalogu \"css\"", "Error");
					}
					catch (System.IO.DirectoryNotFoundException)
					{
						MessageBox.Show("Nieprawid??owa ??cie??ka. Nie mo??na utworzy?? katalogu \"css\"", "Error");
					}
					catch (System.NotSupportedException)
					{
						MessageBox.Show("Nieprawid??owy format ??cie??ki. Nie mo??na utworzy?? katalogu \"css\"", "Error");
					}
					catch (System.IO.IOException)
					{
						MessageBox.Show("Nieprawid??owa ??cie??ka. Nie mo??na utworzy?? katalogu \"css\"", "Error");
					}
				}
				string path = this.path + "\\tmp\\printTmp\\print.html";
				System.IO.StreamWriter sw = new System.IO.StreamWriter(path, false);
				if (this.isSigned)
				{
					string cavas = "<img class=\"divFooter\" src=\"" + this.path + "\\tmp\\printTmp\\code.png\">";
					string script = "<script>jQuery( document ).ready(function(){ var i=0; jQuery('div.break').each(function(){ i++; var p = jQuery(this).position();  var x = (1114*i) - p.top;   jQuery(this).css('margin-bottom', x);});}); </script>";
					sw.Write(string.Concat(new string[]
					{
						this.header,
						this.body,
						cavas,
						script,
						this.footer
					}));
				}
				else
				{
					string script = "<script> jQuery( document ).ready(function(){ var i=0; jQuery('#breakTab').each(function(){ i++; var p = jQuery(this).position();  var x = (950*i) - p.top;   jQuery(this).css('margin-bottom', x);});  jQuery('#breakTab1').each(function(){ i++; var p = jQuery(this).position();  var x = (950*i) - p.top;   jQuery(this).css('margin-bottom', x);}); jQuery('#breakTab3').each(function(){ i++; var p = jQuery(this).position();  var x = (950*i) - p.top;   jQuery(this).css('margin-bottom', x);}); jQuery('#breakTab2').each(function(){ i++; var p = jQuery(this).position();  var x = (950*i) - p.top;   jQuery(this).css('margin-bottom', x);});});  </script>";
					sw.Write(this.header + this.body + script + this.footer);
				}
				sw.Close();
				result = path;
				return result;
			}
			catch (System.UnauthorizedAccessException)
			{
				MessageBox.Show("Nie mozna przygotowac protoko??u do druku. otw??rz aplikacje jako Administrator", "Uwaga");
			}
			catch (System.Exception ex)
			{
				MessageBox.Show("Nie mozna przygotowac protoko??u do druku. Orginal exception: " + ex.Message, "Error");
			}
			result = "";
			return result;
		}
		public string getPreView()
		{
			string path = this.path + "\\tmp\\printTmp\\preview.html";
			System.IO.StreamWriter sw = new System.IO.StreamWriter(path, false);
			string script = "<script> jQuery( document ).ready(function(){ var i=0; jQuery('#breakTab').each(function(){ i++; var p = jQuery(this).position();  var x = (950*i) - p.top;   jQuery(this).css('margin-bottom', x);});  jQuery('#breakTab1').each(function(){ i++; var p = jQuery(this).position();  var x = (950*i) - p.top;   jQuery(this).css('margin-bottom', x);}); jQuery('#breakTab3').each(function(){ i++; var p = jQuery(this).position();  var x = (950*i) - p.top;   jQuery(this).css('margin-bottom', x);}); jQuery('#breakTab2').each(function(){ i++; var p = jQuery(this).position();  var x = (950*i) - p.top;   jQuery(this).css('margin-bottom', x);});});  </script>";
			sw.Write(this.header + this.body + script + this.footer);
			sw.Close();
			return path;
		}
	}
}
